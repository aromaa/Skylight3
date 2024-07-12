using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Public;
using Skylight.API.Numerics;
using Skylight.Server.Collections.Immutable;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.Map.Public;

internal sealed class PublicRoomMap : RoomMap, IPublicRoomMap
{
	private readonly ImmutableArray2D<IPublicRoomTile> tiles;

	internal PublicRoomMap(IRoomLayout layout)
		: base(layout)
	{
		ImmutableArray2D<IPublicRoomTile>.Builder builder = ImmutableArray2D.CreateBuilder<IPublicRoomTile>(layout.Size.X, layout.Size.Y);
		for (int x = 0; x < layout.Size.X; x++)
		{
			for (int y = 0; y < layout.Size.Y; y++)
			{
				builder[x, y] = new PublicRoomTile(this, new Point2D(x, y), ((RoomLayout)layout).Tiles[x, y]);
			}
		}

		this.tiles = builder.MoveToImmutable();
	}

	public override IPublicRoomTile GetTile(int x, int y) => this.tiles[x, y];
	public override IPublicRoomTile GetTile(Point2D point) => this.tiles[point.X, point.Y];
}
