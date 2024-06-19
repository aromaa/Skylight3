using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class BasicFloorFurniture(int id, int width, int length, double height, int stateCount) : FixedHeightMultiStateFloorFurniture(id, width, length, height, stateCount), IBasicFloorFurniture;
