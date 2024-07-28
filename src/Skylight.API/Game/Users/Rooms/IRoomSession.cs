using Skylight.API.Collections.Cache;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Users.Rooms;

public interface IRoomSession
{
	public IUser User { get; }

	public SessionState State { get; }

	public int InstanceType { get; }
	public int InstanceId { get; }
	public int WorldId { get; }

	public IRoom? Room { get; }
	public IUserRoomUnit? Unit { get; }

	public bool TryChangeState(SessionState value, SessionState current);

	public void LoadRoom(ICacheValue<IRoom> roomValue);
	public void EnterRoom(IUserRoomUnit unit);

	public bool Close();
	public void OnClose();

	public enum SessionState : uint
	{
		Connected,
		DoorbellRinging,
		Ready,
		EnterRoom,
		InRoom,
		Disconnected
	}
}
