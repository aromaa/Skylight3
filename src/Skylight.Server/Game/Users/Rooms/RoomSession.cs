using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Outgoing.Room.Permissions;
using Skylight.Protocol.Packets.Outgoing.Room.Session;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Users.Rooms;

internal sealed partial class RoomSession : IRoomSession
{
	private IRoomSession.SessionState state;

	public IUser User { get; }

	public int InstanceType { get; }
	public int InstanceId { get; }
	public int WorldId { get; }

	public IRoom? Room { get; set; }
	public IUserRoomUnit? Unit { get; set; }

	internal RoomSession(IUser user, int instanceType, int instanceId, int worldId)
	{
		this.state = IRoomSession.SessionState.Connected;

		this.User = user;

		this.InstanceType = instanceType;
		this.InstanceId = instanceId;
		this.WorldId = worldId;
	}

	public IRoomSession.SessionState State => this.state;

	private IRoomSession.SessionState ChangeState(IRoomSession.SessionState value)
	{
		return InterlockedExtensions.Exchange(ref this.state, value);
	}

	public bool TryChangeState(IRoomSession.SessionState value, IRoomSession.SessionState current)
	{
		return InterlockedExtensions.CompareExchange(ref this.state, value, current) == current;
	}

	public void LoadRoom(IRoom room)
	{
		if (this.Room is not null)
		{
			throw new NotSupportedException();
		}

		if (!this.TryChangeState(IRoomSession.SessionState.Ready, IRoomSession.SessionState.Connected))
		{
			return;
		}

		this.Room = room;

		this.User.SendAsync(new RoomReadyOutgoingPacket(room.Map.Layout.Id, room.Info.Id));
		this.User.SendAsync(new YouAreControllerOutgoingPacket(room.Info.Id, 4)); //0 = No rights, 1 = Basic rights, 2 = Can place, 3 = Can pickup, 4 = Can remove rights, 5 = IDK
		this.User.SendAsync(new YouAreOwnerOutgoingPacket(room.Info.Id));
	}

	public void EnterRoom(IUserRoomUnit unit)
	{
		if (this.Unit is not null)
		{
			throw new NotSupportedException();
		}

		this.Unit = unit;
	}

	public bool Close() => this.User.CloseRoomSession(this);

	public void OnClose()
	{
		IRoomSession.SessionState old = this.ChangeState(IRoomSession.SessionState.Disconnected);
		if (old != IRoomSession.SessionState.InRoom)
		{
			return;
		}

		this.Room!.PostTask(room =>
		{
			room.Exit(this.User);

			room.UnitManager.RemoveUnit(this.Unit!);
		});
	}
}
