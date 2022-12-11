using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Incoming.Room.Session;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Session;
using Skylight.Protocol.Packets.Outgoing.RoomSettings;

namespace Skylight.Server.Game.Communication.Room.Session;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class OpenFlatConnectionPacketHandler<T> : UserPacketHandler<T>
	where T : IOpenFlatConnectionIncomingPacket
{
	private readonly IRoomManager roomManager;

	public OpenFlatConnectionPacketHandler(IRoomManager roomManager)
	{
		this.roomManager = roomManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession is { State: <= IRoomSession.SessionState.Ready } roomSession)
		{
			if (roomSession.RoomId == packet.RoomId)
			{
				return;
			}
		}

		IRoomSession session = user.OpenRoomSession(packet.RoomId);

		user.Client.ScheduleTask(new OpenFlatTask
		{
			RoomManager = this.roomManager,

			Session = session
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct OpenFlatTask : IClientTask
	{
		internal IRoomManager RoomManager { get; init; }

		internal IRoomSession Session { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			client.SendAsync(new OpenConnectionOutgoingPacket(this.Session.RoomId));

			IRoom? room = await this.RoomManager.GetRoomAsync(this.Session.RoomId).ConfigureAwait(false);
			if (room is null)
			{
				if (this.Session.Close())
				{
					client.SendAsync(new NoSuchFlatOutgoingPacket(this.Session.RoomId));
					client.SendAsync(new CloseConnectionOutgoingPacket());
				}

				return;
			}

			this.Session.LoadRoom(room);
		}
	}
}
