using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Public;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Private;
using Skylight.Domain.Rooms.Public;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Layout;
using Skylight.Server.Game.Rooms.Private;
using Skylight.Server.Game.Rooms.Public;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	: IRoomManager
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;

	private readonly ConcurrentDictionary<int, IPrivateRoom> loadedPrivateRooms = new();
	private readonly ConcurrentDictionary<int, IPublicRoomInstance> loadedPublicInstances = new();

	public IEnumerable<IRoom> LoadedRooms => this.loadedPrivateRooms.Values;

	public async ValueTask<IPrivateRoom?> GetPrivateRoomAsync(int id, CancellationToken cancellationToken)
	{
		if (this.loadedPrivateRooms.TryGetValue(id, out IPrivateRoom? room))
		{
			return room;
		}

		IRoomInfo? roomInfo = await this.navigatorManager.GetRoomDataAsync(id, cancellationToken).ConfigureAwait(false);
		if (roomInfo is null)
		{
			return null;
		}

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		//TODO: Clean up
		CustomRoomLayoutEntity? customLayout = await dbContext.CustomRoomLayouts.FirstOrDefaultAsync(e => e.RoomId == id, cancellationToken).ConfigureAwait(false);

		ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(PrivateRoom),
		[
			typeof(RoomData),
			typeof(IRoomLayout)
		]);

		room = (PrivateRoom)roomFactory(this.serviceProvider,
		[
			roomInfo,
			customLayout is null ? roomInfo.Layout : new RoomLayout(roomInfo.Layout.Id, customLayout.HeightMap, customLayout.DoorX, customLayout.DoorY, customLayout.DoorDirection)
		]);

		this.loadedPrivateRooms.TryAdd(id, room);

		await room.LoadAsync(cancellationToken).ConfigureAwait(false);

		((PrivateRoom)room).Start();

		return room;
	}

	public async ValueTask<IPublicRoomInstance?> GetPublicRoomAsync(int instanceId, CancellationToken cancellationToken = default)
	{
		if (this.loadedPublicInstances.TryGetValue(instanceId, out IPublicRoomInstance? instance))
		{
			return instance;
		}

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		PublicRoomEntity? publicRoom = await dbContext.PublicRooms.FirstOrDefaultAsync(e => e.Id == instanceId, cancellationToken).ConfigureAwait(false);
		if (publicRoom is null)
		{
			return null;
		}

		List<IPublicRoom> rooms = [];
		await foreach (PublicRoomWorldEntity world in dbContext.PublicRoomWorlds
			.Where(e => e.RoomId == instanceId)
			.AsNoTracking()
			.AsAsyncEnumerable()
			.WithCancellation(cancellationToken)
			.ConfigureAwait(false))
		{
			if (!this.navigatorManager.TryGetLayout(world.LayoutId, out IRoomLayout? layout))
			{
				continue;
			}

			ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(PublicRoom),
			[
				typeof(RoomData),
				typeof(IRoomLayout)
			]);

			PublicRoom room = (PublicRoom)roomFactory(this.serviceProvider,
			[
				new RoomData(new PrivateRoomEntity
				{
					Id = publicRoom.Id,
					Name = publicRoom.Name
				}, null!, layout),
				layout
			]);

			await room.LoadAsync(cancellationToken).ConfigureAwait(false);

			room.Start();

			rooms.Add(room);
		}

		instance = new PublicRoomInstance([.. rooms]);

		this.loadedPublicInstances.TryAdd(instanceId, instance);

		return instance;
	}

	public async ValueTask<IPublicRoom?> GetPublicRoomAsync(int instanceId, int worldId, CancellationToken cancellationToken = default)
	{
		IPublicRoomInstance? instance = await this.GetPublicRoomAsync(instanceId, cancellationToken).ConfigureAwait(false);
		if (instance is null)
		{
			return null;
		}

		return instance.Rooms[0];
	}

	public bool TryGetRoom(int id, [NotNullWhen(true)] out IPrivateRoom? room)
	{
		if (this.loadedPrivateRooms.TryGetValue(id, out room))
		{
			return true;
		}

		return false;
	}
}
