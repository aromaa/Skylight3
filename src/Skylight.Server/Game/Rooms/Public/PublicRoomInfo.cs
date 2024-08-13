using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Public;

namespace Skylight.Server.Game.Rooms.Public;

internal sealed class PublicRoomInfo(IPublicRoomInstance instance, int worldId, IRoomLayout layout) : RoomInfo(layout), IPublicRoomInfo
{
	public IPublicRoomInstance Instance { get; } = instance;

	public override int Id => this.Instance.Id;
	public int WorldId { get; } = worldId;
}
