using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Furniture;

internal abstract class FurnitureItemBuilder<TFurniture, TTarget, TBuilder> : IFurnitureItemBuilder<TFurniture, TTarget, TBuilder>
	where TFurniture : IFurniture
	where TTarget : IFurnitureItem<TFurniture>
	where TBuilder : FurnitureItemBuilder<TFurniture, TTarget, TBuilder>
{
	protected int IdValue { get; set; }
	protected TFurniture? FurnitureValue { get; set; }
	protected IUserInfo? OwnerValue { get; set; }
	protected JsonDocument? ExtraDataValue { get; set; }

	public TBuilder Id(int id)
	{
		ArgumentOutOfRangeException.ThrowIfZero(id);

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
		if (this.IdValue == 0)
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
}
