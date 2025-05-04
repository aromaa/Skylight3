using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class GetRoomEntryDataPacketHandler<T> : UserPacketHandler<T>
	where T : IGetRoomEntryDataIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession is not { } roomSession)
		{
			return;
		}

		roomSession.EnterRoom();
	}
}
