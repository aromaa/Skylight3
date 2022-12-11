using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetUserEventCatsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetUserEventCatsIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new UserEventCatsOutgoingPacket(new List<EventCategoryData>
		{
			new(1, "Visible", true),
			new(2, "Hidden", false)
		}));
	}
}
