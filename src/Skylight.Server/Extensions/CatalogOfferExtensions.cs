using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Purse;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Data.Room.Object;

namespace Skylight.Server.Extensions;

internal static class CatalogOfferExtensions
{
	internal static CatalogOfferData BuildOfferData(this ICatalogOffer offer, IRegistry<ICurrencyType> currencyRegistry)
	{
		List<CatalogProductData> products = [];

		foreach (ICatalogProduct product in offer.Products)
		{
			if (product is IFurnitureCatalogProduct furnitureProduct)
			{
				products.Add(new CatalogProductData
				{
					Type = furnitureProduct.Furniture is IFloorFurniture ? FurnitureType.Floor : FurnitureType.Wall,
					FurnitureId = furnitureProduct.Furniture.Id,
					ExtraData = string.Empty,
					ProductCount = furnitureProduct.Amount,
					Expiration = -1
				});
			}
			else if (product is IBadgeCatalogProduct badgeProduct)
			{
				products.Add(new CatalogProductData(FurnitureType.Badge, 0, badgeProduct.Badge.Code, 0, 0));
			}
		}

		CurrencyTypes.Credits.TryGet(currencyRegistry, out ISimpleCurrencyType? creditCurrencyType);
		CurrencyTypes.ActivityPoints.TryGet(currencyRegistry, out ICompoundCurrencyType<IActivityPointsCurrency>? activityPointsCurrencyType);

		int costCredits = 0;
		int costActivityPoints = 0;
		int costActivityPointsType = 0;
		foreach ((ICurrency currency, int amount) in offer.Cost)
		{
			if (currency.Type == creditCurrencyType)
			{
				costCredits = amount;
			}
			else if (currency.Type == activityPointsCurrencyType && amount > costActivityPoints)
			{
				costActivityPoints = amount;
				costActivityPointsType = ((IActivityPointsCurrency)currency).Kind;
			}
		}

		return new CatalogOfferData
		{
			Id = offer.Id,
			LocalizationId = offer.Name,
			IsRent = offer.RentTime > TimeSpan.Zero,
			PriceInCredits = costCredits,
			PriceInActivityPoints = costActivityPoints,
			ActivityPointsType = costActivityPointsType,
			Giftable = false,
			Products = products,
			ClubLevel = offer.PermissionRequirement?.GetClubLevel() ?? 0,
			BundlePurchaseAllowed = offer.HasOffer,
			PreviewImage = string.Empty
		};
	}
}
