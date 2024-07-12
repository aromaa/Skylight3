using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Rooms.Public;

namespace Skylight.API.Game.Rooms;

public interface IRoomManager
{
	public IEnumerable<IRoom> LoadedRooms { get; }

	public ValueTask<IPublicRoomInstance?> GetPublicRoomAsync(int instanceId, CancellationToken cancellationToken = default);
	public ValueTask<IPublicRoom?> GetPublicRoomAsync(int instanceId, int worldId, CancellationToken cancellationToken = default);

	public ValueTask<IPrivateRoom?> GetPrivateRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
