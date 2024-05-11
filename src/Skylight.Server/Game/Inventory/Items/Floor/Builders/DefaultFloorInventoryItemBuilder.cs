using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;

namespace Skylight.Server.Game.Inventory.Items.Floor.Builders;

internal sealed class DefaultFloorInventoryItemBuilder : FloorInventoryItemBuilder<IFloorFurniture, IFloorInventoryItem, DefaultFloorInventoryItemBuilder>
{
	public override IFloorInventoryItem Build()
	{
		this.CheckValid();

		return new DefaultFloorInventoryItem(this.IdValue, this.OwnerValue, this.FurnitureValue);
	}
}
