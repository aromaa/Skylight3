using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.Server.Game.Rooms;

internal abstract class RoomInfo(IRoomLayout layout) : IRoomInfo
{
	public abstract int Id { get; }

	public IRoomLayout Layout { get; } = layout;

	public int UserCount { get; set; }
}
