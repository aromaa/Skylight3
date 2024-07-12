using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Private;
using Skylight.API.Numerics;
using Skylight.Server.Collections.Immutable;
using Skylight.Server.Game.Rooms.Layout;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms.Map.Private;

internal sealed class PrivateRoomMap : RoomMap, IPrivateRoomMap
{
	private readonly ImmutableArray2D<IPrivateRoomTile> tiles;

	internal PrivateRoomMap(PrivateRoom room, IRoomLayout layout)
		: base(layout)
	{
		ImmutableArray2D<IPrivateRoomTile>.Builder builder = ImmutableArray2D.CreateBuilder<IPrivateRoomTile>(layout.Size.X, layout.Size.Y);
		for (int x = 0; x < layout.Size.X; x++)
		{
			for (int y = 0; y < layout.Size.Y; y++)
			{
				builder[x, y] = new PrivateRoomTile(room, this, new Point2D(x, y), ((RoomLayout)layout).Tiles[x, y]);
			}
		}

		this.tiles = builder.MoveToImmutable();
	}

	public override IPrivateRoomTile GetTile(int x, int y) => this.tiles[x, y];
	public override IPrivateRoomTile GetTile(Point2D point) => this.tiles[point.X, point.Y];
}
