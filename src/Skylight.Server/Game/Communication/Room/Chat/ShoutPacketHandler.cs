using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Chat;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Chat;

namespace Skylight.Server.Game.Communication.Room.Chat;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class ShoutPacketHandler<T> : UserPacketHandler<T>
	where T : IShoutIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		string message = Encoding.UTF8.GetString(packet.Text);

		((Rooms.Room)roomUnit.Room).SendAsync(new ShoutOutgoingPacket(roomUnit.Id, message, 0, packet.StyleId, -1, Array.Empty<(string, string, bool)>()));
	}
}
