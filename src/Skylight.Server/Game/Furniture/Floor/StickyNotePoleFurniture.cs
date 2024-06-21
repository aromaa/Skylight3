using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class StickyNotePoleFurniture(int id, FloorFurnitureType type, Point2D dimensions, double height) : PlainFloorFurniture(id, type, dimensions, height), IStickyNotePoleFurniture;
