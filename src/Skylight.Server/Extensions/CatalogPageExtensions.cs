using Skylight.API.Game.Catalog;
using Skylight.Protocol.Packets.Data.Catalog;

namespace Skylight.Server.Extensions;

internal static class CatalogPageExtensions
{
	internal static List<CatalogOfferData> BuildOffersData(this ICatalogPage page)
	{
		List<CatalogOfferData> offers = [];

		foreach (ICatalogOffer offer in page.Offers)
		{
			offers.Add(offer.BuildOfferData());
		}

		return offers;
	}
}
