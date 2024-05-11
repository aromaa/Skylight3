using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items.Wall;

namespace Skylight.Server.Game.Inventory.Items.Wall.Builders;

internal sealed class DefaultWallInventoryItemBuilder : WallInventoryItemBuilder<IWallFurniture, IWallInventoryItem, DefaultWallInventoryItemBuilder>
{
	public override IWallInventoryItem Build()
	{
		this.CheckValid();

		return new DefaultWallInventoryItem(this.IdValue, this.OwnerValue, this.FurnitureValue);
	}
}
