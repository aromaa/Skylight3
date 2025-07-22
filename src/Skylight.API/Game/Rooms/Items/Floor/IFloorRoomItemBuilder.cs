using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor;

public interface IFloorRoomItemBuilder : IRoomItemBuilder;

public interface IFloorRoomItemBuilder<out TTarget> : IFloorRoomItemBuilder, IRoomItemBuilder<TTarget>
	where TTarget : IFloorRoomItem
{
	public IFloorRoomItemBuilder<TTarget> Position(Point3D position);
	public IFloorRoomItemBuilder<TTarget> Direction(int direction);
}

public interface IFloorRoomItemBuilder<in TFurniture, out TTarget> : IFloorRoomItemBuilder<TTarget>, IRoomItemBuilder<TFurniture, TTarget>
	where TFurniture : IFloorFurniture
	where TTarget : IFloorRoomItem, IFurnitureItem<TFurniture>
{
	public new IFloorRoomItemBuilder<TFurniture, TTarget> Position(Point3D position);
	public new IFloorRoomItemBuilder<TFurniture, TTarget> Direction(int direction);

	IFloorRoomItemBuilder<TTarget> IFloorRoomItemBuilder<TTarget>.Position(Point3D position) => this.Position(position);
	IFloorRoomItemBuilder<TTarget> IFloorRoomItemBuilder<TTarget>.Direction(int direction) => this.Direction(direction);
}

public interface IFloorRoomItemBuilder<in TFurniture, out TTarget, out TBuilder> : IFloorRoomItemBuilder<TFurniture, TTarget>, IRoomItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFloorFurniture
	where TTarget : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TBuilder>
{
	public new TBuilder Position(Point3D position);
	public new TBuilder Direction(int direction);

	IFloorRoomItemBuilder<TFurniture, TTarget> IFloorRoomItemBuilder<TFurniture, TTarget>.Position(Point3D position) => this.Position(position);
	IFloorRoomItemBuilder<TFurniture, TTarget> IFloorRoomItemBuilder<TFurniture, TTarget>.Direction(int direction) => this.Direction(direction);
}

public interface IFloorRoomItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IFloorRoomItemBuilder<TFurniture, TTarget, TBuilder>, IRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IFloorFurniture
	where TTarget : IFloorRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IFloorRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TDataBuilder, TBuilder>;
