using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetProductOfferPacketHandler<T>(IRegistryHolder registryHolder, ICatalogManager catalogManager) : UserPacketHandler<T>
	where T : IGetProductOfferIncomingPacket
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	private readonly ICatalogManager catalogManager = catalogManager;

	internal override void Handle(IUser user, in T packet)
	{
		int offerId = packet.OfferId;

		user.Client.ScheduleTask(async _ =>
		{
			ICatalogSnapshot catalogSnapshot = await this.catalogManager.GetAsync().ConfigureAwait(false);

			//TODO: Check against page
			if (!catalogSnapshot.TryGetOffer(offerId, out ICatalogOffer? offer))
			{
				return;
			}

			user.SendAsync(new ProductOfferOutgoingPacket(offer.BuildOfferData(this.registryHolder.Registry(RegistryTypes.Currency))));
		});
	}
}
