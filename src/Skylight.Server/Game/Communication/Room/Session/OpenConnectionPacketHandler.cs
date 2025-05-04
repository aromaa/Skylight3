using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Incoming.Room.Session;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Session;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class OpenConnectionPacketHandler<T> : UserPacketHandler<T>
	where T : IOpenConnectionIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (!user.TryOpenRoomSession(packet.InstanceType, packet.InstanceId, out IRoomSession? session))
		{
			return;
		}

		user.Client.ScheduleTask(async _ => await session.OpenRoomAsync().ConfigureAwait(false));
	}
}
