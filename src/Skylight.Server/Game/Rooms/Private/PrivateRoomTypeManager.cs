using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.Domain.Rooms.Layout;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.Private;

internal sealed class PrivateRoomTypeManager : RoomTypeManager, IRoomTypeManager<IPrivateRoom, IPrivateRoomInfo, int>
{
	private readonly IServiceProvider serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly INavigatorManager navigatorManager;

	private readonly ConcurrentDictionary<int, LoadedPrivateRoom> loadedPrivateRooms = new();

	private readonly Lock unloadQueueLock = new();
	private readonly OrderedDictionary<long, LoadedPrivateRoom> unloadQueue = [];

	internal PrivateRoomTypeManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	{
		this.serviceProvider = serviceProvider;
		this.dbContextFactory = dbContextFactory;
		this.navigatorManager = navigatorManager;

		_ = this.ProcessUnloadQueueAsync();
	}

	public IEnumerable<IRoom> LoadedRooms => this.LoadedInstances;

	public IEnumerable<IPrivateRoom> LoadedInstances => this.loadedPrivateRooms.Values.Select(lr => lr.Room).Where(r => r is not null)!;

	public async ValueTask<ICacheReference<IPrivateRoom>?> GetInstanceAsync(int id, CancellationToken cancellationToken)
	{
		if (this.loadedPrivateRooms.TryGetValue(id, out LoadedPrivateRoom? loadedRoom))
		{
			if (loadedRoom.TryAcquireTicket())
			{
				ValueTask<IPrivateRoom> roomTask = loadedRoom.RoomTask;
				if (roomTask.IsCompletedSuccessfully)
				{
					return new RoomTicket<IPrivateRoom, LoadedPrivateRoom>(loadedRoom);
				}
				else
				{
					//In case of exception, the load initializing thread takes care of the cleanup.
					await loadedRoom.RoomTask.ConfigureAwait(false);

					return new RoomTicket<IPrivateRoom, LoadedPrivateRoom>(loadedRoom);
				}
			}

			//This is a rare path. The room was just unloaded, perform remove to ensure we add a new instance.
			this.loadedPrivateRooms.TryRemove(KeyValuePair.Create(id, loadedRoom));
		}

		ICacheReference<IPrivateRoomInfo>? roomInfoValue = await this.navigatorManager.GetPrivateRoomInfoUnsafeAsync(id, cancellationToken).ConfigureAwait(false);
		if (roomInfoValue is null)
		{
			return null;
		}

		while (true)
		{
			loadedRoom = this.loadedPrivateRooms.GetOrAdd(id, static (_, roomManager) => new LoadedPrivateRoom(roomManager), this);
			if (loadedRoom.TryAcquireTicket())
			{
				try
				{
					//The "ownership" of the room info is handed over here, we don't need to worry about disposing it.
					await loadedRoom.LoadAsync(this.serviceProvider, this.dbContextFactory, roomInfoValue).ConfigureAwait(false);

					return new RoomTicket<IPrivateRoom, LoadedPrivateRoom>(loadedRoom);
				}
				catch
				{
					//Room load failed, remove it. Don't release the ticket however to avoid the room going to the unload queue.
					this.loadedPrivateRooms.TryRemove(KeyValuePair.Create(id, loadedRoom));

					throw;
				}
			}

			//This is a rare path. The room was just unloaded, perform remove to ensure we add a new instance.
			this.loadedPrivateRooms.TryRemove(KeyValuePair.Create(id, loadedRoom));
		}
	}

	public bool TryGetInstance(int roomId, [NotNullWhen(true)] out IPrivateRoom? instance)
	{
		if (this.loadedPrivateRooms.TryGetValue(roomId, out LoadedPrivateRoom? loadedRoom) && loadedRoom.Room is { } roomInstance)
		{
			instance = roomInstance;
			return true;
		}

		instance = null;
		return false;
	}

	private void QueueUnload(LoadedPrivateRoom loadedRoomData)
	{
		lock (this.unloadQueueLock)
		{
			this.unloadQueue.Add(Environment.TickCount64 + (10 * 1000), loadedRoomData);
		}
	}

	private async Task ProcessUnloadQueueAsync()
	{
		PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
		while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
		{
			lock (this.unloadQueueLock)
			{
				long now = Environment.TickCount64;

				if (this.unloadQueue.Count > 0)
				{
					(long unloadTime, LoadedPrivateRoom loadedRoomData) = this.unloadQueue.GetAt(0);
					if (unloadTime > now)
					{
						continue;
					}

					loadedRoomData.TryUnload();

					this.unloadQueue.RemoveAt(0);
				}
			}
		}
	}

	private sealed class LoadedPrivateRoom(PrivateRoomTypeManager manager) : TicketTracked<IPrivateRoom>
	{
		private readonly PrivateRoomTypeManager manager = manager;

		private object? value = new RoomLoadHandler();
		private ICacheReference<IPrivateRoomInfo>? roomInfo;

		internal override IPrivateRoom? Room
		{
			get
			{
				object value = this.value!;

				return value.GetType() == typeof(RoomLoadHandler)
					? null
					: Unsafe.As<IPrivateRoom?>(value);
			}
		}

		internal ValueTask<IPrivateRoom> RoomTask
		{
			get
			{
				object value = this.value!;

				return value.GetType() == typeof(RoomLoadHandler)
					? new ValueTask<IPrivateRoom>(Unsafe.As<RoomLoadHandler>(value).Task)
					: ValueTask.FromResult(Unsafe.As<IPrivateRoom>(value));
			}
		}

		internal Task LoadAsync(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, ICacheReference<IPrivateRoomInfo> roomInfoValue)
		{
			if (this.value is RoomLoadHandler loadHandler)
			{
				return loadHandler.LoadAsync(this, serviceProvider, dbContextFactory, roomInfoValue);
			}

			roomInfoValue.Dispose();

			return Task.CompletedTask;
		}

		protected override void QueueUnload() => this.manager.QueueUnload(this);

		internal void TryUnload()
		{
			if (!this.TryChangeStateToUnloading())
			{
				return;
			}

			IPrivateRoom room = (IPrivateRoom)this.value!;

			//TODO: Activate hybrid mode, stop ticking
			//TODO: Save

			if (!this.TryChangeStateToKilled())
			{
				return;
			}

			//Nobody requested access to the room while saving, we are now free to unload the room.

			this.value = null;

			this.manager.loadedPrivateRooms.TryRemove(KeyValuePair.Create(this.roomInfo!.Value.Id, this));

			room.Unload();

			this.roomInfo.Dispose();
		}

		private sealed class RoomLoadHandler
		{
			private readonly TaskCompletionSource<IPrivateRoom> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

			private volatile bool initialized;

			internal Task<IPrivateRoom> Task => this.taskCompletionSource.Task;

			internal Task LoadAsync(LoadedPrivateRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, ICacheReference<IPrivateRoomInfo> roomInfoValue, CancellationToken cancellationToken = default)
			{
				if (this.initialized || Interlocked.CompareExchange(ref this.initialized, true, false))
				{
					roomInfoValue.Dispose();

					return this.taskCompletionSource.Task;
				}

				return this.InternalLoadAsync(instance, serviceProvider, dbContextFactory, roomInfoValue, cancellationToken);
			}

			private async Task InternalLoadAsync(LoadedPrivateRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, ICacheReference<IPrivateRoomInfo> roomInfoValue, CancellationToken cancellationToken = default)
			{
				try
				{
					IPrivateRoomInfo roomInfo = roomInfoValue.Value;

					await using SkylightContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

					//TODO: Clean up
					CustomRoomLayoutEntity? customLayout = await dbContext.CustomRoomLayouts.FirstOrDefaultAsync(e => e.RoomId == roomInfo.Id, cancellationToken).ConfigureAwait(false);

					ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(PrivateRoom),
					[
						typeof(IPrivateRoomInfo),
						typeof(IRoomLayout)
					]);

					PrivateRoom room = (PrivateRoom)roomFactory(serviceProvider,
					[
						roomInfo,
						customLayout is null ? roomInfo.Layout : new RoomLayout(roomInfo.Layout.Id, customLayout.HeightMap, customLayout.DoorX, customLayout.DoorY, customLayout.DoorDirection)
					]);

					await room.LoadAsync(cancellationToken).ConfigureAwait(false);

					room.Start();

					instance.value = room;
					instance.roomInfo = roomInfoValue;

					this.taskCompletionSource.SetResult(room);
				}
				catch (Exception ex)
				{
					roomInfoValue.Dispose();

					this.taskCompletionSource.SetException(ex);

					throw;
				}
			}
		}
	}
}
