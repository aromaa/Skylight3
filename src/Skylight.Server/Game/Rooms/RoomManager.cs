using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Public;
using Skylight.Domain.Rooms.Public;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Rooms;

internal sealed partial class RoomManager : IRoomManager
{
	private readonly IServiceProvider serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly INavigatorManager navigatorManager;

	private readonly ConcurrentDictionary<int, LoadedPrivateRoom> loadedPrivateRooms = new();
	private readonly ConcurrentDictionary<int, LoadedPublicInstance> loadedPublicInstances = new();

	private readonly OrderedDictionary<long, LoadedPrivateRoom> unloadQueue = [];
	private readonly Lock unloadQueueLock = new();

	public RoomManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	{
		this.serviceProvider = serviceProvider;

		this.dbContextFactory = dbContextFactory;

		this.navigatorManager = navigatorManager;

		_ = this.ProcessUnloadQueueAsync();
	}

	public IEnumerable<IRoom> LoadedRooms
	{
		get
		{
			foreach (LoadedPublicInstance loadedInstance in this.loadedPublicInstances.Values)
			{
				foreach (LoadedPublicRoom loadedWorld in loadedInstance.LoadedWorlds)
				{
					if (loadedWorld.Room is { } room)
					{
						yield return room;
					}
				}
			}

			foreach (IPrivateRoom privateRoom in this.LoadedPrivateRooms)
			{
				yield return privateRoom;
			}
		}
	}

	public IEnumerable<IPrivateRoom> LoadedPrivateRooms
	{
		get
		{
			foreach (LoadedPrivateRoom loadedRoom in this.loadedPrivateRooms.Values)
			{
				if (loadedRoom.Room is { } room)
				{
					yield return room;
				}
			}
		}
	}

	public async ValueTask<ICacheReference<IPrivateRoom>?> GetPrivateRoomAsync(int id, CancellationToken cancellationToken)
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

	public async ValueTask<ICacheReference<IPublicRoomInstance>?> GetPublicRoomAsync(int instanceId, CancellationToken cancellationToken = default)
	{
		if (this.loadedPublicInstances.TryGetValue(instanceId, out LoadedPublicInstance? loadedInstance))
		{
			if (loadedInstance.TryAcquireTicket())
			{
				return new RoomTicket<IPublicRoomInstance, LoadedPublicInstance>(loadedInstance);
			}
		}

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		PublicRoomEntity? publicRoom = await dbContext.PublicRooms.FirstOrDefaultAsync(e => e.Id == instanceId, cancellationToken).ConfigureAwait(false);
		if (publicRoom is null)
		{
			return null;
		}

		while (true)
		{
			loadedInstance = this.loadedPublicInstances.GetOrAdd(instanceId, static (_, state) => new LoadedPublicInstance(state.RoomManager, state.PublicRoom), (RoomManager: this, PublicRoom: publicRoom));
			if (loadedInstance.TryAcquireTicket())
			{
				return new RoomTicket<IPublicRoomInstance, LoadedPublicInstance>(loadedInstance);
			}

			//This is a rare path. The whole instance was just unloaded, perform remove to ensure we add a new instance.
			this.loadedPublicInstances.TryRemove(KeyValuePair.Create(instanceId, loadedInstance));
		}
	}

	public async ValueTask<ICacheReference<IPublicRoom>?> GetPublicRoomAsync(int instanceId, int worldId, CancellationToken cancellationToken = default)
	{
		if (this.loadedPublicInstances.TryGetValue(instanceId, out LoadedPublicInstance? loadedInstance))
		{
			if (loadedInstance.GetWorldAsync(worldId, out LoadedPublicRoom? loadedRoom))
			{
				if (loadedRoom.TryAcquireTicket())
				{
					ValueTask<IPublicRoom> roomTask = loadedRoom.RoomTask;
					if (roomTask.IsCompletedSuccessfully)
					{
						return new RoomTicket<IPublicRoom, LoadedPublicRoom>(loadedRoom);
					}
					else
					{
						//In case of exception, the load initializing thread takes care of the cleanup.
						await loadedRoom.RoomTask.ConfigureAwait(false);

						return new RoomTicket<IPublicRoom, LoadedPublicRoom>(loadedRoom);
					}
				}

				//This is a rare path. The room was just unloaded, perform remove to ensure we add a new instance.
				loadedInstance.RemoveUnloadedWorld(worldId, loadedRoom);
			}

			if (loadedInstance.TryAcquireTicket())
			{
				await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

				PublicRoomWorldEntity? world = await dbContext.PublicRoomWorlds.FirstOrDefaultAsync(e => e.RoomId == instanceId && e.WorldId == worldId, cancellationToken).ConfigureAwait(false);
				if (world is null)
				{
					return null;
				}

				//The ticket is handed over.
				return await loadedInstance.LoadAsync(this.serviceProvider, this.dbContextFactory, world).ConfigureAwait(false);
			}
			else
			{
				//This is a rare path. The whole instance was just unloaded, perform remove to ensure we add a new instance.
				this.loadedPublicInstances.TryRemove(KeyValuePair.Create(instanceId, loadedInstance));
			}
		}

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			PublicRoomEntity? publicRoom = await dbContext.PublicRooms.FirstOrDefaultAsync(e => e.Id == instanceId, cancellationToken).ConfigureAwait(false);
			if (publicRoom is null)
			{
				return null;
			}

			PublicRoomWorldEntity? world = await dbContext.PublicRoomWorlds.FirstOrDefaultAsync(e => e.RoomId == instanceId && e.WorldId == worldId, cancellationToken).ConfigureAwait(false);
			if (world is null)
			{
				return null;
			}

			while (true)
			{
				loadedInstance = this.loadedPublicInstances.GetOrAdd(instanceId, static (_, state) => new LoadedPublicInstance(state.RoomManager, state.PublicRoom), (RoomManager: this, PublicRoom: publicRoom));
				if (loadedInstance.TryAcquireTicket())
				{
					//The ticket is handed over.
					return await loadedInstance.LoadAsync(this.serviceProvider, this.dbContextFactory, world).ConfigureAwait(false);
				}

				//This is a rare path. The whole instance was just unloaded, perform remove to ensure we add a new instance.
				this.loadedPublicInstances.TryRemove(KeyValuePair.Create(instanceId, loadedInstance));
			}
		}
	}

	public bool TryGetPrivateRoom(int roomId, [NotNullWhen(true)] out IPrivateRoom? room)
	{
		if (this.loadedPrivateRooms.TryGetValue(roomId, out LoadedPrivateRoom? loadedRoom) && loadedRoom.Room is { } roomInstance)
		{
			room = roomInstance;
			return true;
		}

		room = null;
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

	private sealed class RoomTicket<TRoom, TInstance>(TInstance loadedRoom) : ICacheReference<TRoom>
		where TInstance : TicketTracked<TRoom>
	{
		private TInstance? loadedRoom = loadedRoom;

#if DEBUG
		~RoomTicket()
		{
			Debug.Assert(this.loadedRoom is null, "Ticket was not disposed");
		}
#endif

		public TRoom Value
		{
			get
			{
				ObjectDisposedException.ThrowIf(this.loadedRoom is null, this);

				return this.loadedRoom.Room!;
			}
		}

		public ICacheReference<TRoom> Retain()
		{
			ObjectDisposedException.ThrowIf(this.loadedRoom is null, this);
			ObjectDisposedException.ThrowIf(!this.loadedRoom.TryAcquireTicket(), this);

			return new RoomTicket<TRoom, TInstance>(this.loadedRoom);
		}

		public void Dispose()
		{
			ObjectDisposedException.ThrowIf(this.loadedRoom is null, this);

			this.loadedRoom.ReleaseTicket();
			this.loadedRoom = null;
		}
	}

	private abstract class TicketTracked
	{
		private const uint Killed = 1u << 31;
		private const uint Unloading = 1u << 30;
		private const uint Mask = TicketTracked.Killed | TicketTracked.Unloading;

		//Bit 32 means we have unloaded, Bit 31 that we are performing unload.
		private volatile uint ticketsCount;

		internal bool TryAcquireTicket()
		{
			while (true)
			{
				uint ticketsCount = this.ticketsCount;
				if ((ticketsCount & TicketTracked.Killed) == 0)
				{
					//We are allowed the clear the unloading bit but NOT the killed.
					uint newTicketsCount = (ticketsCount & ~TicketTracked.Unloading) + 1;

					if (Interlocked.CompareExchange(ref this.ticketsCount, newTicketsCount, ticketsCount) == ticketsCount)
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}
		}

		internal void ReleaseTicket()
		{
			Debug.Assert(this.ticketsCount > 0, "Tickets count overflow");
			Debug.Assert((this.ticketsCount & TicketTracked.Mask) == 0, "Ticket released when unloading");

			if (Interlocked.Decrement(ref this.ticketsCount) == 0)
			{
				this.QueueUnload();
			}
		}

		protected abstract void QueueUnload();

		protected bool TryChangeStateToUnloading() => Interlocked.CompareExchange(ref this.ticketsCount, LoadedPrivateRoom.Unloading, 0) == 0;
		protected bool TryChangeStateToKilled() => Interlocked.CompareExchange(ref this.ticketsCount, LoadedPrivateRoom.Killed, LoadedPrivateRoom.Unloading) == LoadedPrivateRoom.Unloading;
	}

	private abstract class TicketTracked<T> : TicketTracked
	{
		internal abstract T? Room { get; }
	}
}
