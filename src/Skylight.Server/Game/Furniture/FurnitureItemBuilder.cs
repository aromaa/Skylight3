using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Furniture;

internal abstract class FurnitureItemBuilder<TFurniture, TItemId, TTarget, TBuilder> : IFurnitureItemBuilder<TFurniture, TItemId, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TItemId : struct
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : FurnitureItemBuilder<TFurniture, TItemId, TTarget, TBuilder>
{
	protected TItemId IdValue { get; set; }
	protected TFurniture? FurnitureValue { get; set; }
	protected IUserInfo? OwnerValue { get; set; }
	protected JsonDocument? ExtraDataValue { get; set; }

	public TBuilder Id(TItemId id)
	{
		if (!this.ValidId(id))
		{
			throw new ArgumentException("Not valid id", nameof(id));
		}

		this.IdValue = id;

		return (TBuilder)this;
	}

	public TBuilder Furniture(TFurniture furniture)
	{
		this.FurnitureValue = furniture;

		return (TBuilder)this;
	}

	public TBuilder Owner(IUserInfo owner)
	{
		this.OwnerValue = owner;

		return (TBuilder)this;
	}

	public TBuilder ExtraData(JsonDocument extraData)
	{
		this.ExtraDataValue = extraData;

		return (TBuilder)this;
	}

	public abstract TTarget Build();

	[MemberNotNull(nameof(this.FurnitureValue), nameof(this.OwnerValue))]
	protected virtual void CheckValid()
	{
		if (!this.ValidId(this.IdValue))
		{
			throw new InvalidOperationException("Missing id");
		}

		if (this.FurnitureValue is null)
		{
			throw new InvalidOperationException("Missing furniture");
		}

		if (this.OwnerValue is null)
		{
			throw new InvalidOperationException("Missing owner");
		}
	}

	protected abstract bool ValidId(TItemId value);
}
