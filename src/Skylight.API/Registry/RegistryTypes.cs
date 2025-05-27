using Skylight.API.Game.Furniture.Floor;

namespace Skylight.API.Registry;

public static class RegistryTypes
{
	public static readonly RegistryType<IFloorFurnitureKindType> FloorFurnitureKind = new(RegistryRoots.Skylight, ResourceKey.Skylight("floor_furniture_kind"));
}
