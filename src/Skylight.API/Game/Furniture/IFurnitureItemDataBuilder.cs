using System.Text.Json;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureItemDataBuilder
{
	public interface IRaw
	{
		public JsonDocument Build();
	}

	public interface IRaw<out TBuilder> : IRaw, IFurnitureItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IFurnitureItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IFurnitureItemDataBuilder
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IFurnitureItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IFurnitureItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
{
	public TFurnitureBuilder CompleteData();
}
