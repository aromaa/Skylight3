using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class StickyNotePoleFurniture(int id, int width, int length, double height) : PlainFloorFurniture(id, width, length, height), IStickyNotePoleFurniture;
