using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class StatefulFloorFurniture(int id, FloorFurnitureType type, Point2D dimensions) : FloorFurniture(id, type, dimensions), IStatefulFloorFurniture;
