using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Users.Rooms;

public interface IRoomSession
{
	public IUser User { get; }

	public int InstanceType { get; }
	public int InstanceId { get; }
	public int WorldId { get; }

	public IRoom? Room { get; }
	public IUserRoomUnit? Unit { get; }

	public ValueTask OpenRoomAsync();
	public void EnterRoom();

	public bool Close();
}
