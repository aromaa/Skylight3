using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Public;
using Skylight.API.Registry;

namespace Skylight.API.Game.Rooms;

public static class RoomTypes
{
	public static readonly RegistryReference<IRoomType<IPrivateRoom, IPrivateRoomInfo, int>, IRoomType> Private = RoomTypes.Reference<IRoomType<IPrivateRoom, IPrivateRoomInfo, int>>("private");

	public static readonly RegistryReference<IRoomType<IPublicRoomInstance, IPublicRoomInfo, int>, IRoomType> PublicInstance = RoomTypes.Reference<IRoomType<IPublicRoomInstance, IPublicRoomInfo, int>>("public_instance");

	public static readonly RegistryReference<IRoomType<IPublicRoom, IPublicRoomInfo, PublicRoomId>, IRoomType> PublicWorld = RoomTypes.Reference<IRoomType<IPublicRoom, IPublicRoomInfo, PublicRoomId>>("public_world");

	private static RegistryReference<T, IRoomType> Reference<T>(string value)
		where T : IRoomType => new(RegistryTypes.RoomType, ResourceKey.Skylight(value));
}
