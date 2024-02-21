namespace Skylight.API.Game.Inventory.Items.Floor.Builders;

public abstract class FurniMaticGiftInventoryItemBuilder
	: FurnitureInventoryItemBuilder
{
	protected DateTime RecycledAtValue { get; set; }

	public FurniMaticGiftInventoryItemBuilder RecycledAt(DateTime recycledAt)
	{
		this.RecycledAtValue = recycledAt;

		return this;
	}

	public abstract override IFurniMaticGiftInventoryItem Build();
}
