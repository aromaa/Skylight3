using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Inventory.Items;

public abstract class FurnitureInventoryItemBuilder : IFurnitureItemBuilder<IFurnitureInventoryItem>
{
	protected int ItemIdValue { get; set; }

	protected IUserInfo? OwnerValue { get; set; }

	public FurnitureInventoryItemBuilder ItemId(int itemId)
	{
		this.ItemIdValue = itemId;

		return this;
	}

	public abstract FurnitureInventoryItemBuilder Furniture(IFurniture furniture);

	public FurnitureInventoryItemBuilder Owner(IUserInfo owner)
	{
		this.OwnerValue = owner;

		return this;
	}

	public virtual FurnitureInventoryItemBuilder ExtraData(JsonDocument extraData)
	{
		return this;
	}

	public abstract IFurnitureInventoryItem Build();

	[MemberNotNull(nameof(this.OwnerValue))]
	protected virtual void CheckValid()
	{
		ArgumentOutOfRangeException.ThrowIfZero(this.ItemIdValue);
		ArgumentNullException.ThrowIfNull(this.OwnerValue);
	}
}
