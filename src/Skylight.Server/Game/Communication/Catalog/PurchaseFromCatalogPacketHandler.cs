using System.Runtime.InteropServices;
using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class PurchaseFromCatalogPacketHandler<T> : UserPacketHandler<T>
	where T : IPurchaseFromCatalogIncomingPacket
{
	private readonly ICatalogManager catalogManager;

	public PurchaseFromCatalogPacketHandler(ICatalogManager catalogManager)
	{
		this.catalogManager = catalogManager;
	}

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

		bool scheduled = user.Client.ScheduleTask(new CatalogPurchaseTask
		{
			Catalog = catalog,
			Offer = offer,

			ExtraData = Encoding.UTF8.GetString(packet.ExtraData),
			Amount = packet.Amount
		});

		if (!scheduled)
		{
			user.SendAsync(new PurchaseErrorOutgoingPacket(PurchaseErrorReason.Generic));
		}
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct CatalogPurchaseTask : IClientTask
	{
		internal ICatalogSnapshot Catalog { get; init; }
		internal ICatalogOffer Offer { get; init; }

		internal string ExtraData { get; init; }
		internal int Amount { get; init; }

		public Task ExecuteAsync(IClient client) => this.Catalog.PurchaseOfferAsync(client.User!, this.Offer, this.ExtraData, this.Amount);
	}
}
