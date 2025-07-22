using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Floor;

internal abstract class FloorRoomItem<T>(IPrivateRoom room, RoomItemId id, IUserInfo owner, T furniture, Point3D position, int direction) : RoomItem<T>(room, id, owner, furniture), IFloorRoomItem
	where T : IFloorFurniture
{
	public Point3D Position { get; internal set; } = position;
	public int Direction { get; internal set; } = direction;

	public override int StripId => this.Id.Id;

	public new IFloorFurniture Furniture => this.furniture;

	public virtual double Height => this.Furniture.DefaultHeight;

	public EffectiveTilesEnumerator EffectiveTiles => new(this.furniture.EffectiveTiles, this.Direction);

	public virtual void OnMove(Point3D position, int direction)
	{
		this.Position = position;
		this.Direction = direction;
	}
}
