using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Skylight.Domain.Rooms.Private;
using Skylight.Infrastructure;
using Skylight.Server.Collections;
using Skylight.Server.Game.Rooms;
using Skylight.Server.Scheduling;

namespace Skylight.Server.Game.Navigator;

internal sealed class RoomActivityWorker(IDbContextFactory<SkylightContext> dbContextFactory, TimeProvider timeProvider) : BackgroundWorker
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly TimeProvider timeProvider = timeProvider;

	private readonly Lock popularRoomsMutationLock = new();
	private readonly SortedValueSet<int, int> popularRooms = new(Comparer<int>.Default, Comparer<int>.Create((x, y) => -x.CompareTo(y)));

	private readonly ConcurrentDictionary<int, RoomActivity> roomActivity = [];
	private readonly Channel<(int RoomId, int Activity)> roomActivityChannel = Channel.CreateUnbounded<ValueTuple<int, int>>(new UnboundedChannelOptions
	{
		AllowSynchronousContinuations = false,
		SingleReader = true
	});

	private protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			int day = RoomActivity.GetDay(this.timeProvider.GetUtcNow());

			await foreach (var roomActivity in dbContext.PrivateRoomActivity
				.GroupBy(e => e.RoomId)
				.Select(g => new { RoomId = g.Key, Score = g.AsQueryable().Sum(RoomActivity.GetScore(day)) })
				.OrderByDescending(e => e.Score)
				.Take(50)
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				this.UpdateRoomActivity(roomActivity.RoomId, roomActivity.Score);
			}
		}

		PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

		Dictionary<int, PrivateRoomActivityEntity> entities = [];
		while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
		{
			try
			{
				int day = RoomActivity.GetDay(this.timeProvider.GetUtcNow());

				while (this.roomActivityChannel.Reader.TryRead(out (int RoomId, int Activity) value))
				{
					ref PrivateRoomActivityEntity? entity = ref CollectionsMarshal.GetValueRefOrAddDefault(entities, value.RoomId, out _);
					entity ??= new PrivateRoomActivityEntity
					{
						RoomId = value.RoomId,
						Day = day,
					};

					entity.Value += value.Activity;
				}

				if (entities.Count > 0)
				{
					await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

					await dbContext.PrivateRoomActivity
						.UpsertRange(entities.Values)
						.On(e => new { e.RoomId, e.Day })
						.WhenMatched((oldValue, newValue) => new PrivateRoomActivityEntity
						{
							Value = oldValue.Value + newValue.Value,
						}).RunAsync(cancellationToken)
						.ConfigureAwait(false);

					foreach (PrivateRoomActivityEntity entity in entities.Values)
					{
						if (this.roomActivity.TryGetValue(entity.RoomId, out RoomActivity? roomActivity))
						{
							roomActivity.Update(entity.Day, entity.Value);
						}
						else
						{
							IAsyncEnumerable<PrivateRoomActivityEntity> query = dbContext.PrivateRoomActivity
								.Where(e => e.RoomId == entity.RoomId)
								.OrderBy(e => e.Value)
								.AsAsyncEnumerable();

							this.roomActivity[entity.RoomId] = roomActivity = await RoomActivity.LoadAsync(7, 2, day, query).ConfigureAwait(false);
						}

						this.UpdateRoomActivity(entity.RoomId, roomActivity.Score);
					}

					entities.Clear();
				}
			}
			catch (Exception e)
			{
				//TODO: Retry
			}
		}
	}

	private protected override void Complete() => this.roomActivityChannel.Writer.Complete();

	internal void PushRoomActivity(int roomId, int activity)
	{
		this.roomActivityChannel.Writer.WriteAsync((roomId, activity));
	}

	private void UpdateRoomActivity(int roomId, int score)
	{
		const int maxSize = 50;

		if (this.popularRooms.Size >= maxSize && this.popularRooms.Min >= score)
		{
			return;
		}

		lock (this.popularRoomsMutationLock)
		{
			this.popularRooms.UpdateAndTrim(roomId, score, maxSize);
		}
	}

	internal ImmutableSortedSet<(int Activity, int RoomId)> Values => this.popularRooms.Values;
}
