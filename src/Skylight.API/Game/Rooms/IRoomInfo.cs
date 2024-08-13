using Skylight.API.Game.Rooms.Map;

namespace Skylight.API.Game.Rooms;

public interface IRoomInfo
{
	public int Id { get; }

	public IRoomLayout Layout { get; }

	public int UserCount { get; set; }
}
