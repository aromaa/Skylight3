using System.Text.Json;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureItemBuilder;

public interface IFurnitureItemBuilder<in TItemId, out TTarget> : IFurnitureItemBuilder
{
	public IFurnitureItemBuilder<TItemId, TTarget> Id(TItemId id);
	public IFurnitureItemBuilder<TItemId, TTarget> Furniture(IFurniture furniture);
	public IFurnitureItemBuilder<TItemId, TTarget> Owner(IUserInfo owner);
	public IFurnitureItemBuilder<TItemId, TTarget> ExtraData(JsonDocument extraData);
	public TTarget Build();
}

public interface IFurnitureItemBuilder<in TFurniture, in TItemId, out TTarget> : IFurnitureItemBuilder<TItemId, TTarget>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
{
	public IFurnitureItemBuilder<TFurniture, TItemId, TTarget> Furniture(TFurniture furniture);

	IFurnitureItemBuilder<TItemId, TTarget> IFurnitureItemBuilder<TItemId, TTarget>.Furniture(IFurniture furniture) => this.Furniture((TFurniture)furniture);
}

public interface IFurnitureItemBuilder<in TFurniture, in TItemId, out TTarget, out TBuilder> : IFurnitureItemBuilder<TFurniture, TItemId, TTarget>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureItemBuilder<TFurniture, TItemId, TTarget, TBuilder>
{
	public new TBuilder Id(TItemId id);
	public new TBuilder Furniture(TFurniture furniture);
	public new TBuilder Owner(IUserInfo owner);
	public new TBuilder ExtraData(JsonDocument extraData);
	public new TTarget Build();

	IFurnitureItemBuilder<TItemId, TTarget> IFurnitureItemBuilder<TItemId, TTarget>.Id(TItemId id) => this.Id(id);
	IFurnitureItemBuilder<TItemId, TTarget> IFurnitureItemBuilder<TItemId, TTarget>.Owner(IUserInfo owner) => this.Owner(owner);
	IFurnitureItemBuilder<TItemId, TTarget> IFurnitureItemBuilder<TItemId, TTarget>.ExtraData(JsonDocument extraData) => this.ExtraData(extraData);
	IFurnitureItemBuilder<TFurniture, TItemId, TTarget> IFurnitureItemBuilder<TFurniture, TItemId, TTarget>.Furniture(TFurniture furniture) => this.Furniture(furniture);
	TTarget IFurnitureItemBuilder<TItemId, TTarget>.Build() => this.Build();
}

public interface IFurnitureItemBuilder<in TFurniture, in TItemId, out TTarget, out TBuilder, out TDataBuilder> : IFurnitureItemBuilder<TFurniture, TItemId, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : IFurnitureItemBuilder<TFurniture, TItemId, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : IFurnitureItemDataBuilder<TFurniture, TItemId, TTarget, TDataBuilder, TBuilder>
{
	public TDataBuilder Data();
}
