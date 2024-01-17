using Net.Communication.Attributes;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Incoming.Room.Session;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Session;
using Skylight.Protocol.Packets.Outgoing.RoomSettings;

namespace Skylight.Server.Game.Communication.Room.Session;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class OpenFlatConnectionPacketHandler<T> : UserPacketHandler<T>
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

		user.Client.ScheduleTask(async client =>
		{
			client.SendAsync(new OpenConnectionOutgoingPacket(session.RoomId));

			IRoom? room = await this.roomManager.GetRoomAsync(session.RoomId).ConfigureAwait(false);
			if (room is null)
			{
				if (session.Close())
				{
					client.SendAsync(new NoSuchFlatOutgoingPacket(session.RoomId));
					client.SendAsync(new CloseConnectionOutgoingPacket());
				}

				return;
			}

			session.LoadRoom(room);
		});
	}
}
