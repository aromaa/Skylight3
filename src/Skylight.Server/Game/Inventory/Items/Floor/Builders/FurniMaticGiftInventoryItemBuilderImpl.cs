using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Floor.Builders;

namespace Skylight.Server.Game.Inventory.Items.Floor.Builders;

internal sealed class FurniMaticGiftInventoryItemBuilderImpl
	: FurniMaticGiftInventoryItemBuilder
{
	private IFurniMaticGiftFurniture? FurnitureValue { get; set; }

	public override FurniMaticGiftInventoryItemBuilderImpl Furniture(IFurniture furniture)
	{
		this.FurnitureValue = (IFurniMaticGiftFurniture)furniture;

		return this;
	}

	public override FurniMaticGiftInventoryItemBuilderImpl ExtraData(JsonDocument extraData)
	{
		this.RecycledAtValue = extraData.RootElement.GetDateTime();

		return this;
	}

	public override IFurniMaticGiftInventoryItem Build()
	{
		this.CheckValid();

		return new FurniMaticGiftInventoryItem(this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.RecycledAtValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
		ArgumentOutOfRangeException.ThrowIfEqual(this.RecycledAtValue, default);
	}
}
