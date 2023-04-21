using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms.Units;

public interface IUserRoomUnit : IHumanRoomUnit
{
	public IUser User { get; }

	Task LookToAsync(int x, int y);
}
