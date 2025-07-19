using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Purse;

namespace Skylight.API.Registry;

public static class RegistryTypes
{
	public static readonly RegistryType<ICurrencyType> Currency = new(RegistryRoots.Skylight, ResourceKey.Skylight("currency"));

	public static readonly RegistryType<IFloorFurnitureKindType> FloorFurnitureKind = new(RegistryRoots.Skylight, ResourceKey.Skylight("floor_furniture_kind"));
}
