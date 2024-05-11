using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor.Wired;
using Skylight.API.Game.Rooms.Items.Floor.Data.Wired;
using Skylight.API.Game.Rooms.Items.Floor.Wired;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor.Wired;

internal abstract class WiredRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder> : FloorItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>,
	IWiredSelectedItemsRoomItemDataBuilder<TFurniture, TTarget, TDataBuilder, TBuilder>
	where TFurniture : IWiredFurniture
	where TTarget : IWiredRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : WiredRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
	where TDataBuilder : WiredRoomItemBuilder<TFurniture, TTarget, TBuilder, TDataBuilder>
{
	protected List<IFurniture>? SelectedItemsValue { get; set; }

	public IWiredSelectedItemsRoomItemDataBuilder SelectedItems(List<IFurniture> selectedItems)
	{
		this.SelectedItemsValue = selectedItems;

		return this;
	}
}
