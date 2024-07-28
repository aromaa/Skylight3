using Net.Communication.Attributes;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Incoming.Room.Session;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Session;
using Skylight.Protocol.Packets.Outgoing.RoomSettings;

namespace Skylight.Server.Game.Communication.Room.Session;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class OpenConnectionPacketHandler<T>(IRoomManager roomManager) : UserPacketHandler<T>
	where T : IOpenConnectionIncomingPacket
{
	private readonly IRoomManager roomManager = roomManager;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession is { State: <= IRoomSession.SessionState.Ready } roomSession)
		{
			if (roomSession.InstanceType == packet.InstanceType && roomSession.InstanceId == packet.InstanceId)
			{
				return;
			}
		}

		IRoomSession session = user.OpenRoomSession(packet.InstanceType, packet.InstanceId);

		user.Client.ScheduleTask(async client =>
		{
			client.SendAsync(new OpenConnectionOutgoingPacket(session.InstanceId));

			ICacheValue<IRoom>? room = session.InstanceType switch
			{
				0 => await this.roomManager.GetPrivateRoomAsync(session.InstanceId).ConfigureAwait(false),
				1 => await this.roomManager.GetPublicRoomAsync(session.InstanceId, session.WorldId).ConfigureAwait(false),

				_ => null
			};

			if (room is null)
			{
				if (session.Close())
				{
					client.SendAsync(new NoSuchFlatOutgoingPacket(session.InstanceId));
					client.SendAsync(new CloseConnectionOutgoingPacket());
				}

				return;
			}

			session.LoadRoom(room);
		});
	}
}
