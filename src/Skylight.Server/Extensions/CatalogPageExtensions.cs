using Skylight.API.Game.Catalog;
using Skylight.API.Game.Purse;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Data.Catalog;

namespace Skylight.Server.Extensions;

internal static class CatalogPageExtensions
{
	internal static List<CatalogOfferData> BuildOffersData(this ICatalogPage page, IRegistry<ICurrencyType> currencyRegistry)
	{
		List<CatalogOfferData> offers = [];

		foreach (ICatalogOffer offer in page.Offers)
		{
			offers.Add(offer.BuildOfferData(currencyRegistry));
		}

		return offers;
	}
}
