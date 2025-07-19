using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Incoming.Catalog;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Catalog;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Catalog;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetCatalogPagePacketHandler<T>(IRegistryHolder registryHolder, ICatalogManager catalogManager) : UserPacketHandler<T>
	where T : IGetCatalogPageIncomingPacket
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	private readonly ICatalogManager catalogManager = catalogManager;

	internal override void Handle(IUser user, in T packet)
	{
		if (packet.PageId < 0 || !this.catalogManager.TryGetPage(packet.PageId, out ICatalogPage? page) || !page.CanAccess(user))
		{
			return;
		}

		//TODO: Caching
		user.SendAsync(new CatalogPageOutgoingPacket
		{
			PageId = packet.PageId,
			CatalogType = Encoding.ASCII.GetString(packet.CatalogType),
			LayoutCode = page.Layout,
			Images = page.Images,
			Texts = page.Texts,
			Offers = page.BuildOffersData(this.registryHolder.Registry(RegistryTypes.Currency)),
			OfferId = packet.OfferId,
			AcceptSeasonCurrencyAsCredits = false,
			FrontPageItems = Array.Empty<CatalogFrontPageItemData>()
		});
	}
}
