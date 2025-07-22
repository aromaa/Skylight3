using Skylight.API.Registry;

namespace Skylight.API.Game.Rooms.Items;

public static class RoomItemDomains
{
	public static readonly RegistryReference<IRoomItemDomain> BuildersClub = new(RegistryTypes.RoomItemDomain, ResourceKey.Skylight("builders_club"));

	public static readonly RegistryReference<IRoomItemDomain> Normal = new(RegistryTypes.RoomItemDomain, ResourceKey.Skylight("normal"));

	public static readonly RegistryReference<IRoomItemDomain> Transient = new(RegistryTypes.RoomItemDomain, ResourceKey.Skylight("transient"));
}
