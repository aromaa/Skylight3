using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Incoming.Room.Session;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Session;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class OpenFlatConnectionPacketHandler<T> : UserPacketHandler<T>
	where T : IOpenFlatConnectionIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (!user.TryOpenRoomSession(0, packet.RoomId, out IRoomSession? session))
		{
			return;
		}

		user.Client.ScheduleTask(async _ => await session.OpenRoomAsync().ConfigureAwait(false));
	}
}
