using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items;

namespace Skylight.Server.Game.Inventory.Items.Floor.Builders;

internal sealed class SoundSetInventoryItemBuilderImpl
	: FurnitureInventoryItemBuilder
{
	private ISoundSetFurniture? FurnitureValue { get; set; }

	public override SoundSetInventoryItemBuilderImpl Furniture(IFurniture furniture)
	{
		this.FurnitureValue = (ISoundSetFurniture)furniture;

		return this;
	}

	public override IFurnitureInventoryItem Build()
	{
		this.CheckValid();

		return new SoundSetInventoryItem(this.ItemIdValue, this.OwnerValue, this.FurnitureValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
