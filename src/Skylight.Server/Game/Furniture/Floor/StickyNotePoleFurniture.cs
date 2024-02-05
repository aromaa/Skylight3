using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class StickyNotePoleFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IStickyNotePoleFurniture
{
	public override double DefaultHeight => height;
}
