using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Public;
using Skylight.Domain.Rooms.Private;
using Skylight.Domain.Rooms.Public;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Public;

namespace Skylight.Server.Game.Rooms;

internal partial class RoomManager
{
	private sealed class LoadedPublicInstance(RoomManager roomManager, PublicRoomEntity publicRoom) : TicketTracked<IPublicRoomInstance>
	{
		internal RoomManager RoomManager { get; } = roomManager;
		internal PublicRoomEntity PublicRoom { get; } = publicRoom;

		internal override IPublicRoomInstance Room { get; } = new PublicRoomInstance();

		private readonly ConcurrentDictionary<int, LoadedPublicRoom> worlds = new();

		public bool GetWorldAsync(int worldId, [NotNullWhen(true)] out LoadedPublicRoom? loadedRoom) => this.worlds.TryGetValue(worldId, out loadedRoom);

		public void RemoveUnloadedWorld(int worldId, LoadedPublicRoom loadedRoom) => this.worlds.TryRemove(KeyValuePair.Create(worldId, loadedRoom));

		internal async Task<ICacheValue<IPublicRoom>> LoadAsync(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, PublicRoomWorldEntity world)
		{
			while (true)
			{
				LoadedPublicRoom loadedRoom = this.worlds.GetOrAdd(world.WorldId, static (_, state) => new LoadedPublicRoom(state.PublicInstance, state.WorldId), (PublicInstance: this, WorldId: world.WorldId));
				if (loadedRoom.TryAcquireTicket())
				{
					try
					{
						await loadedRoom.LoadAsync(serviceProvider, dbContextFactory, world).ConfigureAwait(false);

						return new RoomTicket<IPublicRoom, LoadedPublicRoom>(loadedRoom);
					}
					catch
					{
						//Room load failed, remove it. Don't release the room ticket however to avoid the room going to the unload queue.
						this.worlds.TryRemove(KeyValuePair.Create(world.WorldId, loadedRoom));

						//But DO release the instance ticket.
						this.ReleaseTicket();

						throw;
					}
				}

				//This is a rare path. The room was just unloaded, perform remove to ensure we add a new instance.
				this.worlds.TryRemove(KeyValuePair.Create(world.WorldId, loadedRoom));
			}
		}

		protected override void QueueUnload()
		{
			if (this.TryChangeStateToUnloading() && this.TryChangeStateToKilled())
			{
				this.RoomManager.loadedPublicInstances.TryRemove(KeyValuePair.Create(this.PublicRoom.Id, this));
			}
		}
	}

	private sealed class LoadedPublicRoom(LoadedPublicInstance publicInstance, int worldId) : TicketTracked<IPublicRoom>
	{
		private readonly LoadedPublicInstance publicInstance = publicInstance;
		private readonly int worldId = worldId;

		private object? value = new RoomLoadHandler();

		internal override IPublicRoom? Room
		{
			get
			{
				object value = this.value!;

				return value.GetType() == typeof(RoomLoadHandler)
					? null
					: Unsafe.As<IPublicRoom?>(value);
			}
		}

		internal ValueTask<IPublicRoom> RoomTask
		{
			get
			{
				object value = this.value!;

				return value.GetType() == typeof(RoomLoadHandler)
					? new ValueTask<IPublicRoom>(Unsafe.As<RoomLoadHandler>(value).Task)
					: ValueTask.FromResult(Unsafe.As<IPublicRoom>(value));
			}
		}

		internal Task LoadAsync(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, PublicRoomWorldEntity world)
		{
			if (this.value is RoomLoadHandler loadHandler)
			{
				return loadHandler.LoadAsync(this, serviceProvider, dbContextFactory, world);
			}

			this.publicInstance.ReleaseTicket();

			return Task.CompletedTask;
		}

		protected override void QueueUnload()
		{
			if (this.TryChangeStateToUnloading() && this.TryChangeStateToKilled())
			{
				IPublicRoom room = (IPublicRoom)this.value!;

				this.value = null;

				this.publicInstance.RemoveUnloadedWorld(this.worldId, this);
				this.publicInstance.ReleaseTicket();

				room.Unload();
			}
		}

		private sealed class RoomLoadHandler
		{
			private readonly TaskCompletionSource<IPublicRoom> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

			private volatile int initialized;

			internal Task<IPublicRoom> Task => this.taskCompletionSource.Task;

			internal Task LoadAsync(LoadedPublicRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, PublicRoomWorldEntity world, CancellationToken cancellationToken = default)
			{
				if (this.initialized != 0 || Interlocked.CompareExchange(ref this.initialized, 1, 0) != 0)
				{
					instance.publicInstance.ReleaseTicket();

					return this.taskCompletionSource.Task;
				}

				return this.InternalLoadAsync(instance, serviceProvider, dbContextFactory, world, cancellationToken);
			}

			private async Task InternalLoadAsync(LoadedPublicRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, PublicRoomWorldEntity world, CancellationToken cancellationToken = default)
			{
				try
				{
					if (!instance.publicInstance.RoomManager.navigatorManager.TryGetLayout(world.LayoutId, out IRoomLayout? layout))
					{
						throw new InvalidOperationException($"Missing room layout data for {world.LayoutId}");
					}

					ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(PublicRoom),
					[
						typeof(RoomData),
						typeof(IRoomLayout)
					]);

					PublicRoom room = (PublicRoom)roomFactory(serviceProvider,
					[
						new RoomData(new PrivateRoomEntity
						{
							Id = instance.publicInstance.PublicRoom.Id,
							Name = instance.publicInstance.PublicRoom.Name
						}, null!, layout),
						layout
					]);

					await room.LoadAsync(cancellationToken).ConfigureAwait(false);

					room.Start();

					instance.value = room;

					this.taskCompletionSource.SetResult(room);
				}
				catch (Exception ex)
				{
					this.taskCompletionSource.SetException(ex);

					throw;
				}
			}
		}
	}
}
