using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Private;

namespace Skylight.API.Game.Rooms.Items;

public interface IRoomItemBuilder : IFurnitureItemBuilder;

public interface IRoomItemBuilder<out TTarget> : IRoomItemBuilder, IFurnitureItemBuilder<RoomItemId, TTarget>
	where TTarget : IRoomItem
{
	public IRoomItemBuilder<TTarget> Room(IPrivateRoom room);
}

public interface IRoomItemBuilder<in TFurniture, out TTarget> : IRoomItemBuilder<TTarget>, IFurnitureItemBuilder<TFurniture, RoomItemId, TTarget>
	where TFurniture : IFurniture
	where TTarget : IRoomItem, IFurnitureItem<TFurniture>
{
	public new IRoomItemBuilder<TFurniture, TTarget> Room(IPrivateRoom room);

	IRoomItemBuilder<TTarget> IRoomItemBuilder<TTarget>.Room(IPrivateRoom room) => this.Room(room);
}

public interface IRoomItemBuilder<in TFurniture, out TTarget, out TBuilder> : IRoomItemBuilder<TFurniture, TTarget>, IFurnitureItemBuilder<TFurniture, RoomItemId, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IRoomItemBuilder<TFurniture, TTarget, TBuilder>
{
	public new TBuilder Room(IPrivateRoom room);

	IRoomItemBuilder<TFurniture, TTarget> IRoomItemBuilder<TFurniture, TTarget>.Room(IPrivateRoom room) => this.Room(room);
}

public interface IRoomItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IRoomItemBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemBuilder<TFurniture, RoomItemId, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IFurniture
	where TTarget : IRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TDataBuilder, TBuilder>;
