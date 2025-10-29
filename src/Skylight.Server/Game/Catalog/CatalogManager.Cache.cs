using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using Skylight.API;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Catalog;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Purse;
using Skylight.API.Registry;
using Skylight.Domain.Catalog;
using Skylight.Domain.Permissions;
using Skylight.Server.Game.Catalog.Products;

namespace Skylight.Server.Game.Catalog;

internal partial class CatalogManager
{
	private sealed class Cache
	{
		internal IRegistry<ICurrencyType> CurrencyRegistry { get; }
		internal IFurnitureSnapshot Furnitures { get; }

		internal FrozenDictionary<int, ICatalogPage> Pages { get; }
		internal FrozenDictionary<int, ICatalogOffer> Offers { get; }

		internal ImmutableArray<ICatalogPage> RootPages { get; }

		private Cache(IRegistry<ICurrencyType> currencyRegistry, IFurnitureSnapshot furnitures, Dictionary<int, ICatalogPage> pages, Dictionary<int, ICatalogOffer> offers, ImmutableArray<ICatalogPage> rootPages)
		{
			this.CurrencyRegistry = currencyRegistry;
			this.Furnitures = furnitures;

			this.Pages = pages.ToFrozenDictionary();
			this.Offers = offers.ToFrozenDictionary();

			this.RootPages = rootPages;
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly List<RetailCatalogEntity> catalogs;

			internal Builder()
			{
				this.catalogs = [];
			}

			internal void AddRetailCatalog(RetailCatalogEntity catalog)
			{
				this.catalogs.Add(catalog);
			}

			internal Cache ToImmutable(IRegistryHolder registryHolder, IFurnitureSnapshot furnitures)
				=> new(registryHolder.Registry(RegistryTypes.Currency), furnitures, [], [], []);

			internal async Task<Cache> ToImmutableAsync(IRegistryHolder registryHolder, IPermissionManager permissionManager, IBadgeSnapshot badges, IFurnitureSnapshot furnitures, CancellationToken cancellationToken)
			{
				IRegistry<ICurrencyType> currencyRegistry = registryHolder.Registry(RegistryTypes.Currency);

				IPermissionDirectory<string> ranksDirectory = await permissionManager.GetRanksDirectoryAsync(cancellationToken).ConfigureAwait(false);

				Dictionary<int, ICatalogPage> catalogPages = [];
				Dictionary<int, ICatalogOffer> catalogOffers = [];

				async Task<CatalogPage> CreatePageAsync(RetailCatalogPageViewEntity pageEntity, ImmutableArray<ImmutableArray<IPermissionSubject>> parentAccess)
				{
					if (pageEntity.Id < 0)
					{
						throw new ArgumentException("Negative ids are not supported by the client.", nameof(pageEntity));
					}

					ImmutableArray<ImmutableArray<IPermissionSubject>> access = await ResolveAccessAsync(pageEntity, parentAccess).ConfigureAwait(false);

					Dictionary<int, ICatalogOffer> offers = [];
					foreach (RetailCatalogPageOfferEntity offerEntity in pageEntity.Offers!)
					{
						CatalogOffer offer = await CreateOfferAsync(offerEntity).ConfigureAwait(false);

						offers.Add(offer.Id, offer);
					}

					OrderedDictionary<int, ICatalogPage> children = [];
					foreach (RetailCatalogPageViewEntity childEntity in pageEntity.Children!)
					{
						CatalogPage child = await CreatePageAsync(childEntity, access).ConfigureAwait(false);

						children.Add(child.Id, child);
					}

					CatalogPageLocalizationEntryEntity localization = pageEntity.Page!.Localization!.Entries!.First();

					CatalogPage page = new(pageEntity.Id, pageEntity.Page!.Localization!.Code, localization.Name, pageEntity.OrderNum, pageEntity.Visiblity > CatalogPageVisiblity.Disabled, pageEntity.Visiblity >= CatalogPageVisiblity.Visible, pageEntity.Page.IconColor, pageEntity.Page.IconImage, pageEntity.Page.Layout, [.. localization.Texts], [.. localization.Images], access, offers, children);
					catalogPages.Add(page.Id, page);

					return page;
				}

				async Task<CatalogOffer> CreateOfferAsync(RetailCatalogPageOfferEntity offerEntity)
				{
					Dictionary<ICurrency, int> cost = [];
					foreach (RetailCatalogOfferCostEntity costEntity in offerEntity.Cost!)
					{
						ICurrency currency = currencyRegistry.Value(ResourceKey.Parse(costEntity.CurrencyType)).Create(costEntity.CurrencyData is { } currencyData ? JsonDocument.Parse(currencyData) : null);

						if (costEntity.Amount <= 0)
						{
							throw new InvalidOperationException($"The offer {offerEntity.Id} has invalid currency cost for {costEntity.CurrencyType}! Amount must be positive.");
						}

						cost[currency] = costEntity.Amount;
					}

					IPermissionSubject? permissionRequirement = null;
					if (offerEntity.RankId is { } rank)
					{
						permissionRequirement = await ranksDirectory.GetSubjectAsync(rank).ConfigureAwait(false);
						if (permissionRequirement is null)
						{
							throw new InvalidOperationException($"The offer {offerEntity.Id} is referring to non-existent rank {rank}!");
						}
					}

					ImmutableArray<ICatalogProduct>.Builder products = ImmutableArray.CreateBuilder<ICatalogProduct>(offerEntity.Offer!.Products!.Count);
					foreach (CatalogProductEntity productEntity in offerEntity.Offer.Products)
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

					CatalogOffer offer = new(offerEntity.Id, offerEntity.PageOrderNum, offerEntity.Offer.Localization!.Code, permissionRequirement, cost.ToFrozenDictionary(), offerEntity.RentTime, offerEntity.BulkDiscount, products.MoveToImmutable());

					catalogOffers.Add(offer.Id, offer);

					return offer;
				}

				async Task<ImmutableArray<ImmutableArray<IPermissionSubject>>> ResolveAccessAsync(RetailCatalogPageViewEntity pageEntity, ImmutableArray<ImmutableArray<IPermissionSubject>> parentAccess)
				{
					if (pageEntity.AccessSet is null)
					{
						return parentAccess;
					}

					ImmutableArray<ImmutableArray<IPermissionSubject>>.Builder access = ImmutableArray.CreateBuilder<ImmutableArray<IPermissionSubject>>(parentAccess.Length + pageEntity.AccessSet.Rules!.Count);
					access.AddRange(parentAccess);

					string? lastPartition = null;
					ImmutableArray<IPermissionSubject>.Builder accessSubjects = ImmutableArray.CreateBuilder<IPermissionSubject>();
					foreach (AccessSetRankRuleEntity accessEntity in pageEntity.AccessSet.Rules)
					{
						if (accessEntity.Partition != lastPartition || accessEntity.Operation == AccessSetRankRuleEntity.OperationType.Or)
						{
							lastPartition = accessEntity.Partition;

							if (accessSubjects.Count > 0)
							{
								access.Add(accessSubjects.DrainToImmutable());
							}
						}

						IPermissionSubject? permissionSubject = await ranksDirectory.GetSubjectAsync(accessEntity.RankId).ConfigureAwait(false);
						if (permissionSubject is null)
						{
							throw new InvalidOperationException($"The page {pageEntity.Id} is referring to non-existent rank {accessEntity.RankId}!");
						}

						accessSubjects.Add(permissionSubject);

						if (accessEntity.Operation == AccessSetRankRuleEntity.OperationType.Or)
						{
							access.Add(accessSubjects.DrainToImmutable());
						}
					}

					if (accessSubjects.Count > 0)
					{
						access.Add(accessSubjects.DrainToImmutable());
					}

					return access.ToImmutable();
				}

				RetailCatalogEntity catalogEntity = this.catalogs.Single();

				ImmutableArray<ICatalogPage>.Builder rootPages = ImmutableArray.CreateBuilder<ICatalogPage>();
				foreach (RetailCatalogPageViewEntity pageEntity in catalogEntity.Views!)
				{
					if (pageEntity.ParentId is null)
					{
						ImmutableArray<ImmutableArray<IPermissionSubject>> access = await ResolveAccessAsync(pageEntity, []).ConfigureAwait(false);

						rootPages.Add(await CreatePageAsync(pageEntity, access).ConfigureAwait(false));
					}
				}

				return new Cache(currencyRegistry, furnitures, catalogPages, catalogOffers, rootPages.DrainToImmutable());
			}
		}
	}
}
