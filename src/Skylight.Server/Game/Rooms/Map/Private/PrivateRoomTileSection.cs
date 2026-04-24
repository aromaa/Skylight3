using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Map.Private;

internal sealed class PrivateRoomTileSection(IPrivateRoom room, Point3D position, bool floor = false) : RoomTileSection(position)
{
	private readonly IPrivateRoom room = room;

	internal bool Floor { get; } = floor;

	internal HashSet<IFloorRoomItem> Items { get; } = [];

	public override void WalkOff(IRoomUnit unit)
	{
		base.WalkOff(unit);

		IFloorRoomItem? item = this.Items.FirstOrDefault();
		if (item is not null && this.room.ItemManager.TryGetInteractionHandler(out IUnitWalkOffTriggerInteractionHandler? handler))
		{
			handler.OnWalkOff((IUserRoomUnit)unit, item);
		}
	}

	public override void WalkOn(IRoomUnit unit)
	{
		base.WalkOn(unit);

		IFloorRoomItem? item = this.Items.FirstOrDefault();
		if (item is not null && this.room.ItemManager.TryGetInteractionHandler(out IUnitWalkOnTriggerInteractionHandler? handler))
		{
			handler.OnWalkOn((IUserRoomUnit)unit, item);
		}
	}
}
