using Skylight.API.Collections.Cache;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Public;

namespace Skylight.API.Game.Rooms;

public interface IRoomManager
{
	public IEnumerable<IRoom> LoadedRooms { get; }

	public ValueTask<ICacheValue<IPublicRoomInstance>?> GetPublicRoomAsync(int instanceId, CancellationToken cancellationToken = default);
	public ValueTask<ICacheValue<IPublicRoom>?> GetPublicRoomAsync(int instanceId, int worldId, CancellationToken cancellationToken = default);

	public ValueTask<ICacheValue<IPrivateRoom>?> GetPrivateRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
