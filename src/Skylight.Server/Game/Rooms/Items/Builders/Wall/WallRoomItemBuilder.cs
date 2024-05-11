using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Builders.Wall;

internal abstract class WallRoomItemBuilder<TFurniture, TTarget, TBuilder> : RoomItemBuilder<TFurniture, TTarget, TBuilder>, IWallRoomItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IWallFurniture
	where TTarget : IWallRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : WallRoomItemBuilder<TFurniture, TTarget, TBuilder>
{
	protected Point2D LocationValue { get; set; }
	protected Point2D PositionValue { get; set; }

	public TBuilder Location(Point2D location)
	{
		this.LocationValue = location;

		return (TBuilder)this;
	}

	public TBuilder Position(Point2D position)
	{
		this.PositionValue = position;

		return (TBuilder)this;
	}
}
