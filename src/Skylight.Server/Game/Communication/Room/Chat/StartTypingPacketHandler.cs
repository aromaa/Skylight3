using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Chat;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Chat;

namespace Skylight.Server.Game.Communication.Room.Chat;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class StartTypingPacketHandler<T> : UserPacketHandler<T>
	where T : IStartTypingIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		((Rooms.Room)roomUnit.Room).SendAsync(new UserTypingOutgoingPacket(roomUnit.Id, true));
	}
}
