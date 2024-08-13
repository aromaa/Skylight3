using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms.Private;

public interface IPrivateRoomInfo : IRoomInfo
{
	public IUserInfo Owner { get; set; }
	public IRoomSettings Settings { get; set; }

	public (IUserInfo Owner, IRoomSettings Settings) Details { get; set; }
}
