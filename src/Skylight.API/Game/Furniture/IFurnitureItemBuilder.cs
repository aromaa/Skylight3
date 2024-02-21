namespace Skylight.API.Game.Furniture;

public interface IFurnitureItemBuilder;

public interface IFurnitureItemBuilder<out TTarget> : IFurnitureItemBuilder
{
	public TTarget Build();
}

public interface IFurnitureItemBuilder<TFurniture, out TTarget> : IFurnitureItemBuilder<TTarget>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>;
