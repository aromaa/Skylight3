using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Private;
using Skylight.API.Numerics;
using Skylight.API.Registry;
using Skylight.Server.Collections.Immutable;
using Skylight.Server.Game.Rooms.Layout;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms.Map.Private;

internal sealed class PrivateRoomMap : RoomMap, IPrivateRoomMap
{
	private readonly ImmutableArray2D<IPrivateRoomTile> tiles;

	private readonly IFloorFurnitureKind? bed;
	private readonly IFloorFurnitureKind? seat;
	private readonly IFloorFurnitureKind? walkable;

	internal PrivateRoomMap(PrivateRoom room, IRoomLayout layout, IRegistryHolder registryHolder)
		: base(layout)
	{
		ImmutableArray2D<IPrivateRoomTile>.Builder builder = ImmutableArray2D.CreateBuilder<IPrivateRoomTile>(layout.Size.X, layout.Size.Y);
		for (int x = 0; x < layout.Size.X; x++)
		{
			for (int y = 0; y < layout.Size.Y; y++)
			{
				builder[x, y] = new PrivateRoomTile(room, this, registryHolder, new Point2D(x, y), ((RoomLayout)layout).Tiles[x, y]);
			}
		}

		this.tiles = builder.MoveToImmutable();

		if (FloorFurnitureKindTypes.Bed.TryGet(registryHolder, out IFloorFurnitureKindType? bedType))
		{
			this.bed = bedType.Value;
		}

		if (FloorFurnitureKindTypes.Seat.TryGet(registryHolder, out IFloorFurnitureKindType? seatType))
		{
			this.seat = seatType.Value;
		}

		if (FloorFurnitureKindTypes.Walkable.TryGet(registryHolder, out IFloorFurnitureKindType? walkableType))
		{
			this.walkable = walkableType.Value;
		}
	}

	public override IPrivateRoomTile GetTile(int x, int y) => this.tiles[x, y];
	public override IPrivateRoomTile GetTile(Point2D point) => this.tiles[point.X, point.Y];

	public override IRoomTileSection? FindSection(IRoomTile tile, Point3D target, double z) => tile.Position.XY == target.XY
		? tile.FindSection(z, f => f.Kind == this.walkable || f.Kind == this.bed || f.Kind == this.seat)
		: tile.FindSection(z, f => f.Kind == this.walkable);
}
