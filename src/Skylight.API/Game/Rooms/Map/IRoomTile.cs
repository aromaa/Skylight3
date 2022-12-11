using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map;

public interface IRoomTile
{
	public Point3D Position { get; }

	public bool IsHole { get; }
	public bool HasRoomUnit { get; }

	public void AddItem(IFloorRoomItem item);

	public void RemoveItem(IFloorRoomItem item);

	public double GetStepHeight(double z);

	public void WalkOn(IRoomUnit unit);
	public void WalkOff(IRoomUnit unit);
}
