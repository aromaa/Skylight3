using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Private;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Items;
using Skylight.Server.Game.Rooms.Map.Private;
using Skylight.Server.Game.Rooms.Units.Private;

namespace Skylight.Server.Game.Rooms.Private;

internal sealed class PrivateRoom : Room, IPrivateRoom
{
	public override IPrivateRoomMap Map { get; }
	public override IRoomUnitManager UnitManager { get; }

	public IRoomItemManager ItemManager { get; }

	public PrivateRoom(RoomData roomData, IRoomLayout roomLayout, IDbContextFactory<SkylightContext> dbContextFactory, IFurnitureManager furnitureManager, IFloorRoomItemStrategy floorRoomItemStrategy, IWallRoomItemStrategy wallRoomItemStrategy, IUserManager userManager, IRoomItemInteractionManager itemInteractionManager, INavigatorManager navigatorManager)
		: base(roomData, roomLayout)
	{
		this.Map = new PrivateRoomMap(this, roomLayout);

		this.ItemManager = new RoomItemManager(this, roomLayout, dbContextFactory, userManager, furnitureManager, floorRoomItemStrategy, wallRoomItemStrategy, itemInteractionManager);

		this.UnitManager = new PrivateRoomUnitManager(this, navigatorManager);
	}

	public override async Task LoadAsync(CancellationToken cancellationToken)
	{
		await this.ItemManager.LoadAsync(cancellationToken).ConfigureAwait(false);
	}

	internal override void DoTick()
	{
		this.ItemManager.Tick();
	}
}
