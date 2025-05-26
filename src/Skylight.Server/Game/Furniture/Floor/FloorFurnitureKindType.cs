using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class FloorFurnitureKindType(IFloorFurnitureKind value) : IFloorFurnitureKindType
{
	public IFloorFurnitureKind Value { get; } = value;
}
