using System.Text.Json;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureItemBuilder;

public interface IFurnitureItemBuilder<out TTarget> : IFurnitureItemBuilder
{
	public IFurnitureItemBuilder<TTarget> Id(int id);
	public IFurnitureItemBuilder<TTarget> Furniture(IFurniture furniture);
	public IFurnitureItemBuilder<TTarget> Owner(IUserInfo owner);
	public IFurnitureItemBuilder<TTarget> ExtraData(JsonDocument extraData);
	public TTarget Build();
}

public interface IFurnitureItemBuilder<in TFurniture, out TTarget> : IFurnitureItemBuilder<TTarget>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
{
	public IFurnitureItemBuilder<TFurniture, TTarget> Furniture(TFurniture furniture);

	IFurnitureItemBuilder<TTarget> IFurnitureItemBuilder<TTarget>.Furniture(IFurniture furniture) => this.Furniture((TFurniture)furniture);
}

public interface IFurnitureItemBuilder<in TFurniture, out TTarget, out TBuilder> : IFurnitureItemBuilder<TFurniture, TTarget>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureItemBuilder<TFurniture, TTarget, TBuilder>
{
	public new TBuilder Id(int id);
	public new TBuilder Furniture(TFurniture furniture);
	public new TBuilder Owner(IUserInfo owner);
	public new TBuilder ExtraData(JsonDocument extraData);
	public new TTarget Build();

	IFurnitureItemBuilder<TTarget> IFurnitureItemBuilder<TTarget>.Id(int id) => this.Id(id);
	IFurnitureItemBuilder<TTarget> IFurnitureItemBuilder<TTarget>.Owner(IUserInfo owner) => this.Owner(owner);
	IFurnitureItemBuilder<TTarget> IFurnitureItemBuilder<TTarget>.ExtraData(JsonDocument extraData) => this.ExtraData(extraData);
	IFurnitureItemBuilder<TFurniture, TTarget> IFurnitureItemBuilder<TFurniture, TTarget>.Furniture(TFurniture furniture) => this.Furniture(furniture);
	TTarget IFurnitureItemBuilder<TTarget>.Build() => this.Build();
}

public interface IFurnitureItemBuilder<in TFurniture, out TTarget, out TBuilder, out TDataBuilder> : IFurnitureItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, TTarget, TDataBuilder, TBuilder>
{
	public TDataBuilder Data();
}
