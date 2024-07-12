using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map.Private;

public interface IPrivateRoomMap : IRoomMap
{
	public new IPrivateRoomTile GetTile(int x, int y);
	public new IPrivateRoomTile GetTile(Point2D location);

	IRoomTile IRoomMap.GetTile(int x, int y) => this.GetTile(x, y);
	IRoomTile IRoomMap.GetTile(Point2D location) => this.GetTile(location);
}
