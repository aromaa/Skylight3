using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class StickyNotePoleFurniture : FloorFurniture, IStickyNotePoleFurniture
{
	public StickyNotePoleFurniture(int id, int width, int length, double height)
		: base(id, width, length, height)
	{
	}
}
