using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms;
using Skylight.Domain.Rooms.Layout;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;
using Skylight.Server.Game.Rooms;

namespace Skylight.Server.Game.Navigator;

internal sealed partial class NavigatorManager : INavigatorManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IUserManager userManager;

	private readonly TypedCache<int, Lazy<Task<IRoomInfo?>>> roomData;

	private Snapshot snapshot;

	public NavigatorManager(IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager)
	{
		this.dbContextFactory = dbContextFactory;

		this.userManager = userManager;

		this.roomData = new TypedCache<int, Lazy<Task<IRoomInfo?>>>();

		this.snapshot = new Snapshot(this, Cache.CreateBuilder().ToImmutable());
	}

	public INavigatorSnapshot Current => this.snapshot;

	public async Task<INavigatorSnapshot> LoadAsync(CancellationToken cancellationToken)
	{
		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (RoomLayoutEntity layout in dbContext.RoomLayouts
						 .AsNoTracking()
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken))
			{
				builder.AddLayout(layout);
			}

			await foreach (RoomFlatCatEntity flatCat in dbContext.FlatCats
							  .AsNoTracking()
							  .AsAsyncEnumerable()
							  .WithCancellation(cancellationToken))
			{
				builder.AddFlatCat(flatCat);
			}
		}

		return this.snapshot = new Snapshot(this, builder.ToImmutable());
	}

	public ValueTask<IRoomInfo?> GetRoomDataAsync(int id, CancellationToken cancellationToken)
	{
		TypedCacheEntry<Lazy<Task<IRoomInfo?>>> value = this.GetCacheEntry(id);

		Task<IRoomInfo?> task = value.Value.Value;
		if (task.IsCompletedSuccessfully)
		{
			return ValueTask.FromResult(task.Result);
		}

		return new ValueTask<IRoomInfo?>(task);
	}

	private TypedCacheEntry<Lazy<Task<IRoomInfo?>>> GetCacheEntry(int id)
	{
		return this.roomData.GetOrAdd(id, static (key, navigatorManager) =>
		{
			Lazy<Task<IRoomInfo?>> value = new(() => navigatorManager.InternalLoadRoomDataAsync(key));

			return new TypedCacheEntry<Lazy<Task<IRoomInfo?>>>(value);
		}, this);
	}

	private async Task<IRoomInfo?> InternalLoadRoomDataAsync(int id)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

		RoomEntity? entity = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == id).ConfigureAwait(false);
		if (entity is null)
		{
			return null;
		}

		if (!this.TryGetLayout(entity.LayoutId, out IRoomLayout? layout))
		{
			throw new InvalidOperationException($"Missing room layout data for {entity.LayoutId}");
		}

		IUserInfo? owner = await this.userManager.GetUserInfoAsync(entity.OwnerId).ConfigureAwait(false);

		return new RoomData(entity, owner!, layout);
	}
}
