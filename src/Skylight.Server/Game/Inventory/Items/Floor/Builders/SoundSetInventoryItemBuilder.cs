using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;

namespace Skylight.Server.Game.Inventory.Items.Floor.Builders;

internal sealed class SoundSetInventoryItemBuilder
	: FloorInventoryItemBuilder<ISoundSetFurniture, ISoundSetInventoryItem, SoundSetInventoryItemBuilder>
{
	public override ISoundSetInventoryItem Build()
	{
		this.CheckValid();

		return new SoundSetInventoryItem(this.IdValue, this.OwnerValue, this.FurnitureValue);
	}
}
