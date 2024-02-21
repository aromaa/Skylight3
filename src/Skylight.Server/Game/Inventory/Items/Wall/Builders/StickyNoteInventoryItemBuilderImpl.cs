using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;
using Skylight.API.Game.Inventory.Items.Wall.Builders;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Inventory.Items.Wall.Builders;

internal sealed class StickyNoteInventoryItemBuilderImpl(IDbContextFactory<SkylightContext> dbContextFactory)
	: StickyNoteInventoryItemBuilder
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private IStickyNoteFurniture? FurnitureValue { get; set; }

	public override StickyNoteInventoryItemBuilderImpl Furniture(IFurniture furniture)
	{
		this.FurnitureValue = (IStickyNoteFurniture)furniture;

		return this;
	}

	public override StickyNoteInventoryItemBuilderImpl ExtraData(JsonDocument extraData)
	{
		this.CountValue = extraData.RootElement.GetInt32();

		return this;
	}

	public override IStickyNoteInventoryItem Build()
	{
		this.CheckValid();

		return new StickyNoteInventoryItem(this.dbContextFactory, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.CountValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
		ArgumentOutOfRangeException.ThrowIfZero(this.CountValue);
	}
}
