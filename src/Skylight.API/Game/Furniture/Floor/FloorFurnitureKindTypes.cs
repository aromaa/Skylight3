using Skylight.API.Registry;

namespace Skylight.API.Game.Furniture.Floor;

public static class FloorFurnitureKindTypes
{
	public static readonly RegistryReference<IFloorFurnitureKindType> Bed = new(RegistryTypes.FloorFurnitureKind, ResourceKey.Skylight("bed"));

	public static readonly RegistryReference<IFloorFurnitureKindType> Obstacle = new(RegistryTypes.FloorFurnitureKind, ResourceKey.Skylight("obstacle"));

	public static readonly RegistryReference<IFloorFurnitureKindType> Seat = new(RegistryTypes.FloorFurnitureKind, ResourceKey.Skylight("seat"));

	public static readonly RegistryReference<IFloorFurnitureKindType> Walkable = new(RegistryTypes.FloorFurnitureKind, ResourceKey.Skylight("walkable"));
}
