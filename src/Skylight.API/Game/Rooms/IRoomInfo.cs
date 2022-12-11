using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms;

public interface IRoomInfo
{
	public int Id { get; }

	public string Name { get; }

	public IUserInfo Owner { get; }

	public IRoomLayout Layout { get; }
}
