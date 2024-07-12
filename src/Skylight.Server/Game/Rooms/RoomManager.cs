using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.Domain.Rooms.Layout;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	: IRoomManager
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;

	private readonly ConcurrentDictionary<int, IRoom> loadedRooms = new();

	public IEnumerable<IRoom> LoadedRooms => this.loadedRooms.Values;

	public async ValueTask<IRoom?> GetRoomAsync(int id, CancellationToken cancellationToken)
	{
		if (this.loadedRooms.TryGetValue(id, out IRoom? room))
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

		ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(Room),
		[
			typeof(RoomData),
			typeof(IRoomLayout)
		]);

		room = (Room)roomFactory(this.serviceProvider,
		[
			roomInfo,
			customLayout is null ? roomInfo.Layout : new RoomLayout(roomInfo.Layout.Id, customLayout.HeightMap, customLayout.DoorX, customLayout.DoorY, customLayout.DoorDirection)
		]);

		this.loadedRooms.TryAdd(id, room);

		await room.LoadAsync(cancellationToken).ConfigureAwait(false);

		return room;
	}

	public bool TryGetRoom(int id, [NotNullWhen(true)] out IRoom? room)
	{
		if (this.loadedRooms.TryGetValue(id, out room))
		{
			return true;
		}

		return false;
	}
}
