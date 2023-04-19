using System.Runtime.CompilerServices;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Outgoing;
using Skylight.Server.Game.Users.Inventory;
using Skylight.Server.Game.Users.Rooms;

namespace Skylight.Server.Game.Users;

internal sealed class User : IUser
{
	public IClient Client { get; }
	public IUserProfile Profile { get; }
	public IUserSettings Settings { get; }
	public IInventory Inventory { get; }

	private IRoomSession? roomSession;

	public User(IClient client, IUserProfile profile, IUserSettings settings)
	{
		this.Client = client;

		this.Profile = profile;
		this.Settings = settings;

		this.Inventory = new UserInventory(this);
	}

	public IRoomSession? RoomSession => this.roomSession;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendAsync<T>(in T packet)
		where T : IGameOutgoingPacket
		=> this.Client.SendAsync(packet);

	public IRoomSession OpenRoomSession(int roomId)
	{
		RoomSession newSession = new(this, roomId);

		IRoomSession? oldSession = Interlocked.Exchange(ref this.roomSession, newSession);
		oldSession?.OnClose();

		return newSession;
	}

	public bool CloseRoomSession(IRoomSession session)
	{
		IRoomSession? oldSession = Interlocked.CompareExchange(ref this.roomSession, null, session);
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
