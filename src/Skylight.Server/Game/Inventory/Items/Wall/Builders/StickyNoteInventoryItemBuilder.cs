using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Inventory.Items.Wall.Data;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Inventory.Items.Wall.Builders;

internal sealed class StickyNoteInventoryItemBuilder(IDbContextFactory<SkylightContext> dbContextFactory) : WallInventoryItemBuilder<IStickyNoteFurniture, IStickyNoteInventoryItem, StickyNoteInventoryItemBuilder>,
	IStickyNoteInventoryItemDataBuilder<IStickyNoteFurniture, IStickyNoteInventoryItem, StickyNoteInventoryItemBuilder, StickyNoteInventoryItemBuilder>,
	IFurnitureItemDataBuilder<IStickyNoteFurniture, int, IStickyNoteInventoryItem, StickyNoteInventoryItemBuilder, StickyNoteInventoryItemBuilder>
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private int CountValue { get; set; }

	public IStickyNoteInventoryItemDataBuilder Count(int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

		this.CountValue = count;

		return this;
	}

	public override IStickyNoteInventoryItem Build()
	{
		this.CheckValid();

		int countValue = this.CountValue;
		if (countValue == 0)
		{
			if (this.ExtraDataValue is not null)
			{
				countValue = this.ExtraDataValue.RootElement.GetInt32();
			}
			else
			{
				throw new InvalidOperationException("You must supply count or extra data");
			}
		}

		return new StickyNoteInventoryItem(this.dbContextFactory, this.IdValue, this.OwnerValue, this.FurnitureValue, countValue);
	}

	public StickyNoteInventoryItemBuilder Data() => this;
	public StickyNoteInventoryItemBuilder CompleteData() => this;
}
