using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall;

public interface IWallRoomItemBuilder : IRoomItemBuilder;

public interface IWallRoomItemBuilder<out TTarget> : IWallRoomItemBuilder, IRoomItemBuilder<TTarget>
	where TTarget : IWallRoomItem
{
	public IWallRoomItemBuilder<TTarget> Location(Point2D location);
	public IWallRoomItemBuilder<TTarget> Position(Point2D position);
}

public interface IWallRoomItemBuilder<in TFurniture, out TTarget> : IWallRoomItemBuilder<TTarget>, IRoomItemBuilder<TFurniture, TTarget>
	where TFurniture : IWallFurniture
	where TTarget : IWallRoomItem, IFurnitureItem<TFurniture>
{
	public new IWallRoomItemBuilder<TFurniture, TTarget> Location(Point2D location);
	public new IWallRoomItemBuilder<TFurniture, TTarget> Position(Point2D position);

	IWallRoomItemBuilder<TTarget> IWallRoomItemBuilder<TTarget>.Location(Point2D location) => this.Location(location);
	IWallRoomItemBuilder<TTarget> IWallRoomItemBuilder<TTarget>.Position(Point2D position) => this.Position(position);
}

public interface IWallRoomItemBuilder<in TFurniture, out TTarget, out TBuilder> : IWallRoomItemBuilder<TFurniture, TTarget>, IRoomItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IWallFurniture
	where TTarget : IWallRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IWallRoomItemBuilder<TFurniture, TTarget, TBuilder>
{
	public new TBuilder Location(Point2D location);
	public new TBuilder Position(Point2D position);

	IWallRoomItemBuilder<TFurniture, TTarget> IWallRoomItemBuilder<TFurniture, TTarget>.Location(Point2D location) => this.Location(location);
	IWallRoomItemBuilder<TFurniture, TTarget> IWallRoomItemBuilder<TFurniture, TTarget>.Position(Point2D position) => this.Position(position);
}

public interface IWallRoomItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IWallRoomItemBuilder<TFurniture, TTarget, TBuilder>, IRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IWallFurniture
	where TTarget : IWallRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IWallRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TDataBuilder, TBuilder>;
