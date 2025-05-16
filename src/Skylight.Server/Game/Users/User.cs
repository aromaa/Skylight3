using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Server.Game.Users.Authentication;
using Skylight.Server.Game.Users.Inventory;
using Skylight.Server.Game.Users.Rooms;

namespace Skylight.Server.Game.Users;

internal sealed class User : IUser
{
	private readonly IRoomManager roomManager;

	private readonly UserInventory inventory;

	public IClient Client { get; }
	public IUserProfile Profile { get; }
	public IPurse Purse { get; }
	public IUserSettings Settings { get; }

	private RoomSession? roomSession;

	public User(IRoomManager roomManager, IClient client, IUserProfile profile, IPurse purse, IUserSettings settings)
	{
		this.roomManager = roomManager;

		this.inventory = new UserInventory(this);

		this.Client = client;
		this.Profile = profile;
		this.Purse = purse;
		this.Settings = settings;
	}

	public IInventory Inventory => this.inventory;
	public IRoomSession? RoomSession => this.roomSession;

	public async Task LoadAsync(SkylightContext dbContext, UserAuthentication.LoadContext loadContext, CancellationToken cancellationToken)
	{
		await this.inventory.LoadAsync(dbContext, loadContext, cancellationToken).ConfigureAwait(false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
		=> this.Client.SendAsync(packet);

	public IRoomSession OpenRoomSession(int instanceType, int instanceId, Func<IRoom, IUser, IUserRoomUnit> unitFactory) => this.OpenRoomSession(instanceType, instanceId, 0, unitFactory);

	public IRoomSession OpenRoomSession(int instanceType, int instanceId, int worldId, Func<IRoom, IUser, IUserRoomUnit> unitFactory)
	{
		RoomSession newSession = new(this.roomManager, this, instanceType, instanceId, worldId, static (room, user) => room.UnitManager.CreateUnit(user));

		RoomSession? oldSession = Interlocked.Exchange(ref this.roomSession, newSession);
		oldSession?.OnClose();

		return newSession;
	}

	public bool TryOpenRoomSession(int instanceType, int instanceId, [NotNullWhen(true)] out IRoomSession? session) => this.TryOpenRoomSession(instanceType, instanceId, 0, out session);

	public bool TryOpenRoomSession(int instanceType, int instanceId, int worldId, [NotNullWhen(true)] out IRoomSession? session)
	{
		if (this.roomSession is { State: <= Rooms.RoomSession.SessionState.Ready } roomSession)
		{
			if (roomSession.InstanceType == instanceType && roomSession.InstanceId == instanceId)
			{
				session = null;
				return false;
			}
		}

		session = this.OpenRoomSession(instanceType, instanceId, worldId, static (room, user) => room.UnitManager.CreateUnit(user));
		return true;
	}

	public bool CloseRoomSession(IRoomSession session)
	{
		RoomSession? oldSession = Interlocked.CompareExchange(ref this.roomSession, null, (RoomSession)session);
		if (oldSession == session)
		{
			oldSession.OnClose();

			return true;
		}

		return false;
	}

	public void Disconnect()
	{
		Interlocked.Exchange(ref this.roomSession, null)?.OnClose();
	}
}
