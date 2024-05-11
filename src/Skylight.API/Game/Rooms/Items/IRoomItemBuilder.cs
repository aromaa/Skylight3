using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Rooms.Items;

public interface IRoomItemBuilder : IFurnitureItemBuilder;

public interface IRoomItemBuilder<out TTarget> : IRoomItemBuilder, IFurnitureItemBuilder<TTarget>
	where TTarget : IRoomItem
{
	public IRoomItemBuilder<TTarget> Room(IRoom room);
}

public interface IRoomItemBuilder<in TFurniture, out TTarget> : IRoomItemBuilder<TTarget>, IFurnitureItemBuilder<TFurniture, TTarget>
	where TFurniture : IFurniture
	where TTarget : IRoomItem, IFurnitureItem<TFurniture>
{
	public new IRoomItemBuilder<TFurniture, TTarget> Room(IRoom room);

	IRoomItemBuilder<TTarget> IRoomItemBuilder<TTarget>.Room(IRoom room) => this.Room(room);
}

public interface IRoomItemBuilder<in TFurniture, out TTarget, out TBuilder> : IRoomItemBuilder<TFurniture, TTarget>, IFurnitureItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IRoomItemBuilder<TFurniture, TTarget, TBuilder>
{
	public new TBuilder Room(IRoom room);

	IRoomItemBuilder<TFurniture, TTarget> IRoomItemBuilder<TFurniture, TTarget>.Room(IRoom room) => this.Room(room);
}

public interface IRoomItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IRoomItemBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TFurniture : IFurniture
	where TTarget : IRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, TTarget, TDataBuilder, TBuilder>;
