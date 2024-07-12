using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory;
using Skylight.API.Game.Users.Rooms;
using Skylight.API.Net;

namespace Skylight.API.Game.Users;

public interface IUser : IPacketSender
{
	public IClient Client { get; }
	public IUserSettings Settings { get; }
	public IUserProfile Profile { get; }
	public IInventory Inventory { get; }
	public IRoomSession? RoomSession { get; }

	public IRoomSession OpenRoomSession(int instanceType, int instanceId, int worldId = 0);
	public bool CloseRoomSession(IRoomSession session);

	public void Disconnect();
}
