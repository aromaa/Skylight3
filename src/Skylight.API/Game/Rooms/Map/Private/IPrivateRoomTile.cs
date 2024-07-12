using Skylight.API.Game.Rooms.Items.Floor;

namespace Skylight.API.Game.Rooms.Map.Private;

public interface IPrivateRoomTile : IRoomTile
{
	public IEnumerable<IFloorRoomItem> FloorItems { get; }

	public IEnumerable<IFloorRoomItem> GetFloorItemsBetween(double minZ, double maxZ);

	public void AddItem(IFloorRoomItem item);
	public void RemoveItem(IFloorRoomItem item);
}
