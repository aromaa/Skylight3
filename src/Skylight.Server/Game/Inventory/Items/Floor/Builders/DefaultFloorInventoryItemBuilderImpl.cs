using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items;

namespace Skylight.Server.Game.Inventory.Items.Floor.Builders;

internal sealed class DefaultFloorInventoryItemBuilderImpl
	: FurnitureInventoryItemBuilder
{
	private IFloorFurniture? FurnitureValue { get; set; }

	public override DefaultFloorInventoryItemBuilderImpl Furniture(IFurniture furniture)
	{
		this.FurnitureValue = (IFloorFurniture)furniture;

		return this;
	}

	public override IFurnitureInventoryItem Build()
	{
		this.CheckValid();

		return new DefaultFloorInventoryItem(this.ItemIdValue, this.OwnerValue, this.FurnitureValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
