using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture.Floor;
using Skylight.Protocol.Packets.Data.Catalog;
using Skylight.Protocol.Packets.Data.Room.Object;

namespace Skylight.Server.Extensions;

internal static class CatalogOfferExtensions
{
	internal static CatalogOfferData BuildOfferData(this ICatalogOffer offer)
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

		return new CatalogOfferData
		{
			Id = offer.Id,
			LocalizationId = offer.Name,
			IsRent = offer.RentTime > TimeSpan.Zero,
			PriceInCredits = offer.CostCredits,
			PriceInActivityPoints = offer.CostActivityPoints,
			ActivityPointsType = offer.ActivityPointsType,
			Giftable = false,
			Products = products,
			ClubLevel = offer.ClubRank,
			BundlePurchaseAllowed = offer.HasOffer,
			PreviewImage = string.Empty
		};
	}
}
