using System.Collections.Frozen;
using System.Collections.Immutable;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.Domain.Catalog;
using Skylight.Server.Game.Catalog.Products;

namespace Skylight.Server.Game.Catalog;

internal partial class CatalogManager
{
	private sealed class Cache
	{
		internal IFurnitureSnapshot Furnitures { get; }

		internal FrozenDictionary<int, ICatalogPage> Pages { get; }
		internal FrozenDictionary<int, ICatalogOffer> Offers { get; }

		internal ImmutableArray<ICatalogPage> RootPages { get; }

		private Cache(IFurnitureSnapshot furnitures, Dictionary<int, ICatalogPage> pages, Dictionary<int, ICatalogOffer> offers, ImmutableArray<ICatalogPage> rootPages)
		{
			this.Furnitures = furnitures;

			this.Pages = pages.ToFrozenDictionary();
			this.Offers = offers.ToFrozenDictionary();

			this.RootPages = rootPages;
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly List<CatalogPageEntity> rootPages;

			internal Builder()
			{
				this.rootPages = new List<CatalogPageEntity>();
			}

			internal void AddPage(CatalogPageEntity page)
			{
				if (page.ParentId is null)
				{
					this.rootPages.Add(page);
				}
			}

			internal Cache ToImmutable(IBadgeSnapshot badges, IFurnitureSnapshot furnitures)
			{
				Dictionary<int, ICatalogPage> catalogPages = new();
				Dictionary<int, ICatalogOffer> catalogOffers = new();

				CatalogPage CreatePage(CatalogPageEntity pageEntity)
				{
					Dictionary<int, ICatalogOffer> offers = new();
					foreach (CatalogOfferEntity offerEntity in pageEntity.Offers!)
					{
						CatalogOffer offer = CreateOffer(offerEntity);

						offers.Add(offer.Id, offer);
					}

					Dictionary<int, ICatalogPage> children = new();
					foreach (CatalogPageEntity childEntity in pageEntity.Children!)
					{
						CatalogPage child = CreatePage(childEntity);

						children.Add(child.Id, child);
					}

					CatalogPage page = new(pageEntity.Id, pageEntity.Name, pageEntity.Localization, pageEntity.OrderNum, pageEntity.Enabled, pageEntity.Visible, pageEntity.MinRank, pageEntity.ClubRank, pageEntity.IconColor, pageEntity.IconImage, pageEntity.Layout, pageEntity.Texts.ToImmutableArray(), pageEntity.Images.ToImmutableArray(), pageEntity.AcceptSeasonCurrencyAsCredits, offers, children);

					catalogPages.Add(page.Id, page);

					return page;
				}

				CatalogOffer CreateOffer(CatalogOfferEntity offerEntity)
				{
					ImmutableArray<ICatalogProduct>.Builder products = ImmutableArray.CreateBuilder<ICatalogProduct>(offerEntity.Products!.Count);
					foreach (CatalogProductEntity productEntity in offerEntity.Products)
					{
						if (productEntity is CatalogFloorProductEntity floorProduct)
						{
							if (!furnitures.TryGetFloorFurniture(floorProduct.FurnitureId, out IFloorFurniture? furniture))
							{
								throw new InvalidOperationException($"The product {productEntity.Id} is referring to non-existent floor item {floorProduct.FurnitureId}!");
							}

							products.Add(new CatalogProductFloorItem(furniture, floorProduct.Amount));
						}
						else if (productEntity is CatalogWallProductEntity wallProduct)
						{
							if (!furnitures.TryGetWallFurniture(wallProduct.FurnitureId, out IWallFurniture? furniture))
							{
								throw new InvalidOperationException($"The product {productEntity.Id} is referring to non-existent wall item {wallProduct.FurnitureId}!");
							}

							//TODO: Factory?
							if (furniture is IStickyNoteFurniture postItFurniture)
							{
								products.Add(new CatalogProductStickyNote(postItFurniture, wallProduct.Amount));
							}
							else
							{
								products.Add(new CatalogProductWallItem(furniture, wallProduct.Amount));
							}
						}
						else if (productEntity is CatalogBadgeProductEntity badgeProduct)
						{
							if (!badges.TryGetBadge(badgeProduct.BadgeCode, out IBadge? badge))
							{
								throw new InvalidOperationException($"The product {productEntity.Id} is referring to non-existent badge {badgeProduct.BadgeCode}!");
							}

							products.Add(new CatalogProductBadge(badge));
						}
						else
						{
							throw new NotSupportedException($"The offer product {productEntity.Id} is missing a floor or wall furniture!");
						}
					}

					CatalogOffer offer = new(offerEntity.Id, offerEntity.Name, offerEntity.OrderNum, offerEntity.ClubRank, offerEntity.CostCredits, offerEntity.CostActivityPoints, offerEntity.ActivityPointsType, offerEntity.RentTime, offerEntity.HasOffer, products.MoveToImmutable());

					catalogOffers.Add(offer.Id, offer);

					return offer;
				}

				ImmutableArray<ICatalogPage>.Builder rootPages = ImmutableArray.CreateBuilder<ICatalogPage>(this.rootPages.Count);
				foreach (CatalogPageEntity pageEntity in this.rootPages)
				{
					rootPages.Add(CreatePage(pageEntity));
				}

				return new Cache(furnitures, catalogPages, catalogOffers, rootPages.MoveToImmutable());
			}
		}
	}
}
