namespace Skylight.API.Game.Inventory.Items.Wall.Builders;

public abstract class StickyNoteInventoryItemBuilder
	: FurnitureInventoryItemBuilder
{
	protected int CountValue { get; set; }

	public StickyNoteInventoryItemBuilder Count(int count)
	{
		this.CountValue = count;

		return this;
	}

	public abstract override IStickyNoteInventoryItem Build();
}
