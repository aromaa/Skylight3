using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users.Rooms;
using Skylight.API.Net;

namespace Skylight.API.Game.Users;

public interface IUser : IPacketSender
{
	public IClient Client { get; }
	public IUserSettings Settings { get; }
	public IUserProfile Profile { get; }
	public IPermissionSubject PermissionSubject { get; }
	public IInventory Inventory { get; }
	public IRoomSession? RoomSession { get; }

	public IRoomSession OpenRoomSession(int instanceType, int instanceId, Func<IRoom, IUser, IUserRoomUnit> unitFactory);
	public IRoomSession OpenRoomSession(int instanceType, int instanceId, int worldId, Func<IRoom, IUser, IUserRoomUnit> unitFactory);

	public bool TryOpenRoomSession(int instanceType, int instanceId, [NotNullWhen(true)] out IRoomSession? session);
	public bool TryOpenRoomSession(int instanceType, int instanceId, int worldId, [NotNullWhen(true)] out IRoomSession? session);

	public bool CloseRoomSession(IRoomSession session);

	public void Disconnect();
}
