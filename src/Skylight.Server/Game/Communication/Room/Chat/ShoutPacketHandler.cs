using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Chat;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Chat;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class ShoutPacketHandler<T> : UserPacketHandler<T>
	where T : IShoutIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		string message = user.Client.Encoding.GetString(packet.Text);
		int styleId = packet.StyleId;

		roomUnit.Room.PostTask(_ =>
		{
			roomUnit.Shout(message, styleId);
		});
	}
}
