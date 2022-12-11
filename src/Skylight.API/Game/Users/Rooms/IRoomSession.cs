using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;

namespace Skylight.API.Game.Users.Rooms;

public interface IRoomSession
{
	public IUser User { get; }

	public SessionState State { get; }

	public int RoomId { get; }

	public IRoom? Room { get; }
	public IUserRoomUnit? Unit { get; }

	public bool TryChangeState(SessionState value, SessionState current);

	public void LoadRoom(IRoom room);
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
