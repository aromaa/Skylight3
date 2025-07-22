using System.Drawing;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.API.Game.Rooms.Items.Wall.Data;

public interface IStickyNoteRoomItemDataBuilder : IFurnitureItemDataBuilder<IStickyNoteFurniture, IStickyNoteRoomItem, IStickyNoteRoomItemDataBuilder>
{
	public IStickyNoteRoomItemDataBuilder Color(Color color);
	public IStickyNoteRoomItemDataBuilder Text(string text);

	public new interface IRaw<out TBuilder> : IFurnitureItemDataBuilder.IRaw<TBuilder>, IStickyNoteRoomItemDataBuilder
		where TBuilder : IRaw<TBuilder>;
}

public interface IStickyNoteRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder> : IStickyNoteRoomItemDataBuilder
	where TFurniture : IStickyNoteFurniture
	where TTarget : IStickyNoteRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IStickyNoteRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IFurnitureItemDataBuilder<TFurniture, TTarget, TBuilder>;

public interface IStickyNoteRoomItemDataBuilder<in TFurniture, out TTarget, out TBuilder, out TFurnitureBuilder> : IStickyNoteRoomItemDataBuilder<TFurniture, TTarget, TBuilder>, IWallRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>
	where TFurniture : IStickyNoteFurniture
	where TTarget : IStickyNoteRoomItem, IFurnitureItem<TFurniture>
	where TBuilder : IStickyNoteRoomItemDataBuilder<TFurniture, TTarget, TBuilder, TFurnitureBuilder>, IFurnitureItemDataBuilder<TFurniture, RoomItemId, TTarget, TBuilder, TFurnitureBuilder>
	where TFurnitureBuilder : IWallRoomItemBuilder<TFurniture, TTarget, TFurnitureBuilder, TBuilder>;
