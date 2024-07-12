using System.Runtime.CompilerServices;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Inventory;
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
	private readonly UserInventory inventory;

	public IClient Client { get; }
	public IUserProfile Profile { get; }
	public IUserSettings Settings { get; }

	private IRoomSession? roomSession;

	public User(IClient client, IUserProfile profile, IUserSettings settings)
	{
		this.inventory = new UserInventory(this);

		this.Client = client;
		this.Profile = profile;
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

	public IRoomSession OpenRoomSession(int instanceType, int instanceId, int worldId)
	{
		RoomSession newSession = new(this, instanceType, instanceId, worldId);

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
