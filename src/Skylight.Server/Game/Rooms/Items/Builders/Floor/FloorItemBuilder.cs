using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal abstract class FloorItemBuilder<TFurniture, TTarget, TBuilder> : RoomItemBuilder<TFurniture, TTarget, TBuilder>, IFloorRoomItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFloorFurniture
	where TTarget : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : FloorItemBuilder<TFurniture, TTarget, TBuilder>
{
	protected Point3D PositionValue { get; set; }
	protected int DirectionValue { get; set; }

	public TBuilder Position(Point3D position)
	{
		this.PositionValue = position;

		return (TBuilder)this;
	}

	public TBuilder Direction(int direction)
	{
		this.DirectionValue = direction;

		return (TBuilder)this;
	}
}

internal abstract class FloorItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder> : FloorItemBuilder<TFurniture, TTarget, TBuilder>,
	IFloorRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>,
	IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TDataBuilder, TBuilder>
	where TFurniture : IFloorFurniture
	where TTarget : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : FloorItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : FloorItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
{
	public TDataBuilder Data() => (TDataBuilder)this;
	public TBuilder CompleteData() => (TBuilder)this;
}
