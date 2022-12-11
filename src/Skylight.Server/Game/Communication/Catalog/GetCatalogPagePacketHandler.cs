using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetCatalogPagePacketHandler<T> : UserPacketHandler<T>
	where T : IGetCatalogPageIncomingPacket
{
	private readonly ICatalogManager catalogManager;

	public GetCatalogPagePacketHandler(ICatalogManager catalogManager)
	{
		this.catalogManager = catalogManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (!this.catalogManager.TryGetPage(packet.PageId, out ICatalogPage? page) || !page.CanAccess(user))
		{
			return;
		}

		//TODO: Caching
		user.SendAsync(new CatalogPageOutgoingPacket
		{
			PageId = packet.PageId,
			CatalogType = Encoding.UTF8.GetString(packet.CatalogType),
			LayoutCode = page.Layout,
			Images = page.Images,
			Texts = page.Texts,
			Offers = page.BuildOffersData(),
			OfferId = packet.OfferId,
			AcceptSeasonCurrencyAsCredits = page.AcceptSeasonCurrencyAsCredits,
			FrontPageItems = Array.Empty<CatalogFrontPageItemData>()
		});
	}
}
