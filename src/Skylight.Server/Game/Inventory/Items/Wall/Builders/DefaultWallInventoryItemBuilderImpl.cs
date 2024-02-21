using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;

namespace Skylight.Server.Game.Inventory.Items.Wall.Builders;

internal sealed class DefaultWallInventoryItemBuilderImpl
	: FurnitureInventoryItemBuilder
{
	private IWallFurniture? FurnitureValue { get; set; }

	public override DefaultWallInventoryItemBuilderImpl Furniture(IFurniture furniture)
	{
		this.FurnitureValue = (IWallFurniture)furniture;

		return this;
	}

	public override IFurnitureInventoryItem Build()
	{
		this.CheckValid();

		return new DefaultWallInventoryItem(this.ItemIdValue, this.OwnerValue, this.FurnitureValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
