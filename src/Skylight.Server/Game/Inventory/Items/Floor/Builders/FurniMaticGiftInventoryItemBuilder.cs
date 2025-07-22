using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Inventory.Items.Floor.Data;

namespace Skylight.Server.Game.Inventory.Items.Floor.Builders;

internal sealed class FurniMaticGiftInventoryItemBuilder : FloorInventoryItemBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, FurniMaticGiftInventoryItemBuilder>,
	IFurniMaticGiftInventoryItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftInventoryItem, FurniMaticGiftInventoryItemBuilder, FurniMaticGiftInventoryItemBuilder>,
	IFurnitureItemDataBuilder<IFurniMaticGiftFurniture, int, IFurniMaticGiftInventoryItem, FurniMaticGiftInventoryItemBuilder, FurniMaticGiftInventoryItemBuilder>
{
	private DateTime RecycledAtValue { get; set; }

	public IFurniMaticGiftInventoryItemDataBuilder Recycled(DateTime recycledAt)
	{
		this.RecycledAtValue = recycledAt;

		return this;
	}

	public override IFurniMaticGiftInventoryItem Build()
	{
		this.CheckValid();

		DateTime recycledAt = this.RecycledAtValue;
		if (recycledAt == default)
		{
			if (this.ExtraDataValue is not null)
			{
				this.RecycledAtValue = this.ExtraDataValue.RootElement.GetDateTime();
			}
			else
			{
				throw new InvalidOperationException("You must recycled at or extra data");
			}
		}

		return new FurniMaticGiftInventoryItem(this.IdValue, this.OwnerValue, this.FurnitureValue, recycledAt);
	}

	public FurniMaticGiftInventoryItemBuilder Data() => this;
	public FurniMaticGiftInventoryItemBuilder CompleteData() => this;
}
