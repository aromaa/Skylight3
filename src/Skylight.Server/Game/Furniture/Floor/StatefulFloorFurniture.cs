using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class StatefulFloorFurniture(int id, int width, int length) : FloorFurniture(id, width, length), IStatefulFloorFurniture;
