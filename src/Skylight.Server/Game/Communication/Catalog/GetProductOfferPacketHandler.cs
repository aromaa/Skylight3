using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetProductOfferPacketHandler<T>(ICatalogManager catalogManager) : UserPacketHandler<T>
	where T : IGetProductOfferIncomingPacket
{
	private readonly ICatalogManager catalogManager = catalogManager;

	internal override void Handle(IUser user, in T packet)
	{
		//TODO: Check against page
		if (!this.catalogManager.TryGetOffer(packet.OfferId, out ICatalogOffer? offer))
		{
			return;
		}

		user.SendAsync(new ProductOfferOutgoingPacket(offer.BuildOfferData()));
	}
}
