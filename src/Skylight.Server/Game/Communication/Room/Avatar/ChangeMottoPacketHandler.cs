using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Avatar;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Avatar;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class ChangeMottoPacketHandler<T> : UserPacketHandler<T>
	where T : IChangeMottoIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		string motto = user.Client.Encoding.GetString(packet.Motto);
		if (motto.Length > 38 || motto == user.Info.Motto)
		{
			return;
		}

		user.Info.Motto = motto;
	}
}
