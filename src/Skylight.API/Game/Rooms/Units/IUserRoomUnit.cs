using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Units;

public interface IUserRoomUnit : IHumanRoomUnit
{
	public IUser User { get; }

	void LookTo(Point2D target);
}
