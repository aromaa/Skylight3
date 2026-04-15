using Skylight.API.Collections.Cache;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Outgoing.Room.Permissions;
using Skylight.Protocol.Packets.Outgoing.Room.Session;
using Skylight.Protocol.Packets.Outgoing.RoomSettings;

namespace Skylight.Server.Game.Users.Rooms;

// TODO: This needs some clean up after all the revisions
// have their room entry sequences examined.
internal sealed partial class RoomSession : IRoomSession
{
	private readonly IRoomManager roomManager;
	private readonly Func<IRoom, IUser, IUserRoomUnit> unitFactory;

	private readonly Lock stateLock;
	private volatile SessionState state;

	private ICacheReference<IRoom>? roomReference;
	private IRoom? room;

	public IUser User { get; }

	public int InstanceType { get; }
	public int InstanceId { get; }
	public int WorldId { get; }

	public IUserRoomUnit? Unit { get; set; }

	internal RoomSession(IRoomManager roomManager, IUser user, int instanceType, int instanceId, int worldId, Func<IRoom, IUser, IUserRoomUnit> unitFactory)
	{
		this.roomManager = roomManager;
		this.unitFactory = unitFactory;

		this.stateLock = new Lock();
		this.state = SessionState.None;

		this.User = user;

		this.InstanceType = instanceType;
		this.InstanceId = instanceId;
		this.WorldId = worldId;
	}

	public IRoom? Room => this.room;

	internal SessionState State => this.state;

	private bool TryChangeState(SessionState value, SessionState comparand)
	{
		SessionState state = this.state;
		if (state == SessionState.Disconnected)
		{
			return false;
		}
		else if (state != comparand)
		{
			throw new InvalidOperationException($"Expected the state to be {comparand}, but was {state}.");
		}

		this.state = value;

		return true;
	}

	public async ValueTask OpenRoomAsync()
	{
		lock (this.stateLock)
		{
			if (!this.TryChangeState(SessionState.Connecting, SessionState.None))
			{
				return;
			}

			this.User.SendAsync(new OpenConnectionOutgoingPacket(this.InstanceId));
		}

		ICacheReference<IRoom>? roomReference = this.InstanceType switch
		{
			0 => await this.roomManager.GetPrivateRoomAsync(this.InstanceId).ConfigureAwait(false),
			1 => await this.roomManager.GetPublicRoomAsync(this.InstanceId, this.WorldId).ConfigureAwait(false),
			_ => null
		};

		if (roomReference is null)
		{
			lock (this.stateLock)
			{
				if (this.Close())
				{
					this.User.SendAsync(new NoSuchFlatOutgoingPacket(this.InstanceId));
					this.User.SendAsync(new CloseConnectionOutgoingPacket());
				}
			}

			return;
		}

		lock (this.stateLock)
		{
			SessionState state = this.state;
			if (state != SessionState.Connecting)
			{
				roomReference.Dispose();

				if (state != SessionState.Disconnected)
				{
					throw new InvalidOperationException($"Expected the state to be {SessionState.Connecting}, but was {state}.");
				}

				return;
			}

			this.state = SessionState.Ready;
			this.roomReference = roomReference;
			this.room = roomReference.Value;

			this.User.SendAsync(new RoomReadyOutgoingPacket(this.room.Map.Layout.Id, this.room.Info.Id));
			this.User.SendAsync(new YouAreControllerOutgoingPacket(this.room.Info.Id, 4)); //0 = No rights, 1 = Basic rights, 2 = Can place, 3 = Can pickup, 4 = Can remove rights, 5 = IDK
			this.User.SendAsync(new YouAreOwnerOutgoingPacket(this.room.Info.Id));
		}
	}

	public void TryEnterRoom()
	{
		lock (this.stateLock)
		{
			if (this.state != SessionState.Ready)
			{
				return;
			}

			this.state = SessionState.EnterRoom;
		}

		this.EnterRoomCore();
	}

	public void EnterRoom()
	{
		lock (this.stateLock)
		{
			if (!this.TryChangeState(SessionState.EnterRoom, SessionState.Ready))
			{
				return;
			}
		}

		this.EnterRoomCore();
	}

	private void EnterRoomCore()
	{
		this.room!.PostTask(room =>
		{
			lock (this.stateLock)
			{
				if (this.state != SessionState.EnterRoom)
				{
					return;
				}

				this.state = SessionState.InRoom;

				room.Enter(this.User);

				this.Unit = this.unitFactory(room, this.User);
			}
		});
	}

	public bool Close() => this.User.CloseRoomSession(this);

	internal void OnClose()
	{
		lock (this.stateLock)
		{
			(SessionState state, this.state) = (this.state, SessionState.Disconnected);
			if (state != SessionState.InRoom)
			{
				this.roomReference?.Dispose();

				return;
			}

			this.room!.PostTask(room =>
			{
				room.Exit(this.User);

				if (this.Unit is { } unit)
				{
					room.UnitManager.RemoveUnit(unit);
				}

				this.roomReference!.Dispose();
			});
		}
	}

	internal enum SessionState : uint
	{
		None,
		Connecting,
		DoorbellRinging,
		Ready,
		EnterRoom,
		InRoom,
		Disconnected
	}
}
