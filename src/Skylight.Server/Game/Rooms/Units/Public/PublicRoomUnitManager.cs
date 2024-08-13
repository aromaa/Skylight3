namespace Skylight.Server.Game.Rooms.Units.Public;

internal sealed class PublicRoomUnitManager(Room room) : RoomUnitManager
{
	protected override Room Room { get; } = room;
}
