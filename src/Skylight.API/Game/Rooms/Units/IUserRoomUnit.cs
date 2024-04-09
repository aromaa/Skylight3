using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Units;

public interface IUserRoomUnit : IHumanRoomUnit
{
	public IUser User { get; }

	public void Chat(string message, int styleId = 0, int trackingId = -1);
	public void Shout(string message, int styleId = 0, int trackingId = -1);

	public void LookTo(Point2D target);
}
