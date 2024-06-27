using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class PurchaseFromCatalogPacketHandler<T>(ICatalogManager catalogManager) : UserPacketHandler<T>
	where T : IPurchaseFromCatalogIncomingPacket
{
	private readonly ICatalogManager catalogManager = catalogManager;

	internal override void Handle(IUser user, in T packet)
	{
		if (packet.Amount is < 1 or > 100)
		{
			user.SendAsync(new PurchaseNotAllowedOutgoingPacket(PurchaseNotAllowedReason.Generic));

			return;
		}

		ICatalogSnapshot catalog = this.catalogManager.Current;

		if (!catalog.TryGetPage(packet.PageId, out ICatalogPage? page)
			|| !page.CanAccess(user)
			|| !page.TryGetOffer(packet.OfferId, out ICatalogOffer? offer))
		{
			user.SendAsync(new PurchaseNotAllowedOutgoingPacket(PurchaseNotAllowedReason.Generic));

			return;
		}
		else if (!offer.CanPurchase(user))
		{
			user.SendAsync(new PurchaseNotAllowedOutgoingPacket(PurchaseNotAllowedReason.NoClubMembership));

			return;
		}

		string extraData = user.Client.Encoding.GetString(packet.ExtraData);
		int amount = packet.Amount;

		bool scheduled = user.Client.ScheduleTask(client =>
		{
			return catalog.PurchaseOfferAsync(client.User!, offer, extraData, amount);
		});

		if (!scheduled)
		{
			user.SendAsync(new PurchaseErrorOutgoingPacket(PurchaseErrorReason.Generic));
		}
	}
}
