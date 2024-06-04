using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.LandingView;
using Skylight.Protocol.Packets.Incoming.LandingView;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.LandingView;

namespace Skylight.Server.Game.Communication.LandingView;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetPromoArticlesPacketHandler<T> : UserPacketHandler<T>
	where T : IGetPromoArticlesIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new PromoArticlesOutgoingPacket(new List<PromoArticleData>
		{
			new(1, "Skylight", "Brought to you by the Skylight Incorpororated.", "GitHub", 0, "https://github.com/aromaa/Skylight3", "catalogue/attic15_catalog_teaser.gif")
		}));
	}
}
