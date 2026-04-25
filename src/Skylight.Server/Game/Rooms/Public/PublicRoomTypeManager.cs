using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Public;
using Skylight.Domain.Rooms.Public;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Rooms.Public;

internal sealed class PublicRoomTypeManager : RoomTypeManager, IRoomTypeManager<IPublicRoomInstance, IPublicRoomInfo, int>, IRoomTypeManager<IPublicRoom, IPublicRoomInfo, PublicRoomId>
{
	private readonly IServiceProvider serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly INavigatorManager navigatorManager;

	private readonly ConcurrentDictionary<int, LoadedPublicInstance> loadedPublicInstances = new();

	internal PublicRoomTypeManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	{
		this.serviceProvider = serviceProvider;
		this.dbContextFactory = dbContextFactory;
		this.navigatorManager = navigatorManager;
	}

	public IEnumerable<IRoom> LoadedRooms => this.LoadedInstances;

	public IEnumerable<IPublicRoom> LoadedInstances => this.loadedPublicInstances.Values.SelectMany(i => i.LoadedWorlds).Select(w => w.Room).Where(r => r is not null)!;

	IEnumerable<IPublicRoomInstance> IRoomTypeManager<IPublicRoomInstance, IPublicRoomInfo, int>.LoadedInstances => this.loadedPublicInstances.Values.Select(i => i.Room);

	public async ValueTask<ICacheReference<IPublicRoomInstance>?> GetInstanceAsync(int instanceId, CancellationToken cancellationToken = default)
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

	public async ValueTask<ICacheReference<IPublicRoom>?> GetInstanceAsync(PublicRoomId roomId, CancellationToken cancellationToken = default)
	{
		if (this.loadedPublicInstances.TryGetValue(roomId.InstanceId, out LoadedPublicInstance? loadedInstance))
		{
			if (loadedInstance.GetWorldAsync(roomId.WorldId, out LoadedPublicRoom? loadedRoom))
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
				loadedInstance.RemoveUnloadedWorld(roomId.WorldId, loadedRoom);
			}

			if (loadedInstance.TryAcquireTicket())
			{
				await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

				PublicRoomWorldEntity? world = await dbContext.PublicRoomWorlds.FirstOrDefaultAsync(e => e.RoomId == roomId.InstanceId && e.WorldId == roomId.WorldId, cancellationToken).ConfigureAwait(false);
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
				this.loadedPublicInstances.TryRemove(KeyValuePair.Create(roomId.InstanceId, loadedInstance));
			}
		}

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			PublicRoomEntity? publicRoom = await dbContext.PublicRooms.FirstOrDefaultAsync(e => e.Id == roomId.InstanceId, cancellationToken).ConfigureAwait(false);
			if (publicRoom is null)
			{
				return null;
			}

			PublicRoomWorldEntity? world = await dbContext.PublicRoomWorlds.FirstOrDefaultAsync(e => e.RoomId == roomId.InstanceId && e.WorldId == roomId.WorldId, cancellationToken).ConfigureAwait(false);
			if (world is null)
			{
				return null;
			}

			while (true)
			{
				loadedInstance = this.loadedPublicInstances.GetOrAdd(roomId.InstanceId, static (_, state) => new LoadedPublicInstance(state.RoomManager, state.PublicRoom), (RoomManager: this, PublicRoom: publicRoom));
				if (loadedInstance.TryAcquireTicket())
				{
					//The ticket is handed over.
					return await loadedInstance.LoadAsync(this.serviceProvider, this.dbContextFactory, world).ConfigureAwait(false);
				}

				//This is a rare path. The whole instance was just unloaded, perform remove to ensure we add a new instance.
				this.loadedPublicInstances.TryRemove(KeyValuePair.Create(roomId.InstanceId, loadedInstance));
			}
		}
	}

	public bool TryGetInstance(int roomId, [NotNullWhen(true)] out IPublicRoomInstance? instance)
	{
		if (this.loadedPublicInstances.TryGetValue(roomId, out LoadedPublicInstance? loadedRoom) && loadedRoom.Room is { } roomInstance)
		{
			instance = roomInstance;
			return true;
		}

		instance = null;
		return false;
	}

	public bool TryGetInstance(PublicRoomId roomId, [NotNullWhen(true)] out IPublicRoom? instance)
	{
		if (this.loadedPublicInstances.TryGetValue(roomId.InstanceId, out LoadedPublicInstance? loadedRoom)
			&& loadedRoom.GetWorldAsync(roomId.WorldId, out LoadedPublicRoom? loadedWorld) && loadedWorld.Room is { } roomInstance)
		{
			instance = roomInstance;
			return true;
		}

		instance = null;
		return false;
	}

	private sealed class LoadedPublicInstance(PublicRoomTypeManager manager, PublicRoomEntity publicRoom) : TicketTracked<IPublicRoomInstance>
	{
		private readonly ConcurrentDictionary<int, LoadedPublicRoom> worlds = new();

		internal PublicRoomTypeManager Manager { get; } = manager;
		internal PublicRoomEntity PublicRoom { get; } = publicRoom;

		internal override IPublicRoomInstance Room { get; } = new PublicRoomInstance(publicRoom.Id);

		internal ICollection<LoadedPublicRoom> LoadedWorlds => this.worlds.Values;

		internal async Task<ICacheReference<IPublicRoom>> LoadAsync(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, PublicRoomWorldEntity world)
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
				this.Manager.loadedPublicInstances.TryRemove(KeyValuePair.Create(this.PublicRoom.Id, this));
			}
		}

		public bool GetWorldAsync(int worldId, [NotNullWhen(true)] out LoadedPublicRoom? loadedRoom) => this.worlds.TryGetValue(worldId, out loadedRoom);

		public void RemoveUnloadedWorld(int worldId, LoadedPublicRoom loadedRoom) => this.worlds.TryRemove(KeyValuePair.Create(worldId, loadedRoom));
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

			private volatile bool initialized;

			internal Task<IPublicRoom> Task => this.taskCompletionSource.Task;

			internal Task LoadAsync(LoadedPublicRoom instance, IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, PublicRoomWorldEntity world, CancellationToken cancellationToken = default)
			{
				if (this.initialized || Interlocked.CompareExchange(ref this.initialized, true, false))
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
					if (!instance.publicInstance.Manager.navigatorManager.TryGetLayout(world.LayoutId, out IRoomLayout? layout))
					{
						throw new InvalidOperationException($"Missing room layout data for {world.LayoutId}");
					}

					ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(PublicRoom),
					[
						typeof(IPublicRoomInfo),
						typeof(IRoomLayout)
					]);

					PublicRoom room = (PublicRoom)roomFactory(serviceProvider,
					[
						new PublicRoomInfo(instance.publicInstance.Room, world.WorldId, layout),
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
