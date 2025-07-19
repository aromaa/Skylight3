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
			private readonly List<CatalogPageEntity> rootPages;

			internal Builder()
			{
				this.rootPages = [];
			}

			internal void AddPage(CatalogPageEntity page)
			{
				if (page.Id < 0)
				{
					throw new ArgumentException("Negative ids are not supported by the client.", nameof(page));
				}

				if (page.ParentId is null)
				{
					this.rootPages.Add(page);
				}
			}

			internal Cache ToImmutable(IRegistryHolder registryHolder, IFurnitureSnapshot furnitures)
				=> new(registryHolder.Registry(RegistryTypes.Currency), furnitures, [], [], []);

			internal async Task<Cache> ToImmutableAsync(IRegistryHolder registryHolder, IPermissionManager permissionManager, IBadgeSnapshot badges, IFurnitureSnapshot furnitures, CancellationToken cancellationToken)
			{
				IRegistry<ICurrencyType> currencyRegistry = registryHolder.Registry(RegistryTypes.Currency);

				IPermissionDirectory<string> ranksDirectory = await permissionManager.GetRanksDirectoryAsync(cancellationToken).ConfigureAwait(false);

				Dictionary<int, ICatalogPage> catalogPages = [];
				Dictionary<int, ICatalogOffer> catalogOffers = [];

				async Task<CatalogPage> CreatePageAsync(CatalogPageEntity pageEntity, ImmutableArray<ImmutableArray<IPermissionSubject>> parentAccess)
				{
					ImmutableArray<ImmutableArray<IPermissionSubject>> access = await ResolveAccessAsync(pageEntity, parentAccess).ConfigureAwait(false);

					Dictionary<int, ICatalogOffer> offers = [];
					foreach (CatalogOfferEntity offerEntity in pageEntity.Offers!)
					{
						CatalogOffer offer = await CreateOfferAsync(offerEntity).ConfigureAwait(false);

						offers.Add(offer.Id, offer);
					}

					OrderedDictionary<int, ICatalogPage> children = [];
					foreach (CatalogPageEntity childEntity in pageEntity.Children!)
					{
						CatalogPage child = await CreatePageAsync(childEntity, access).ConfigureAwait(false);

						children.Add(child.Id, child);
					}

					CatalogPage page = new(pageEntity.Id, pageEntity.Name, pageEntity.Localization, pageEntity.OrderNum, pageEntity.Enabled, pageEntity.Visible, pageEntity.IconColor, pageEntity.IconImage, pageEntity.Layout, [.. pageEntity.Texts], [.. pageEntity.Images], access, offers, children);

					catalogPages.Add(page.Id, page);

					return page;

					async Task<ImmutableArray<ImmutableArray<IPermissionSubject>>> ResolveAccessAsync(CatalogPageEntity pageEntity, ImmutableArray<ImmutableArray<IPermissionSubject>> parentAccess)
					{
						if (pageEntity.Access!.Count <= 0)
						{
							return parentAccess;
						}

						ImmutableArray<ImmutableArray<IPermissionSubject>>.Builder access = ImmutableArray.CreateBuilder<ImmutableArray<IPermissionSubject>>(parentAccess.Length + pageEntity.Access.Count);
						access.AddRange(parentAccess);

						string? lastPartition = null;
						ImmutableArray<IPermissionSubject>.Builder accessSubjects = ImmutableArray.CreateBuilder<IPermissionSubject>();
						foreach (CatalogPageAccessEntity accessEntity in pageEntity.Access)
						{
							if (accessEntity.Partition != lastPartition || accessEntity.Operation == CatalogPageAccessEntity.OperationType.Or)
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

							if (accessEntity.Operation == CatalogPageAccessEntity.OperationType.Or)
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
				}

				async Task<CatalogOffer> CreateOfferAsync(CatalogOfferEntity offerEntity)
				{
					Dictionary<ICurrency, int> cost = [];
					foreach (CatalogOfferCostEntity costEntity in offerEntity.Cost!)
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

					CatalogOffer offer = new(offerEntity.Id, offerEntity.OrderNum, offerEntity.Name, permissionRequirement, cost.ToFrozenDictionary(), offerEntity.RentTime, offerEntity.HasOffer, products.MoveToImmutable());

					catalogOffers.Add(offer.Id, offer);

					return offer;
				}

				ImmutableArray<ICatalogPage>.Builder rootPages = ImmutableArray.CreateBuilder<ICatalogPage>(this.rootPages.Count);
				foreach (CatalogPageEntity pageEntity in this.rootPages)
				{
					rootPages.Add(await CreatePageAsync(pageEntity, []).ConfigureAwait(false));
				}

				return new Cache(currencyRegistry, furnitures, catalogPages, catalogOffers, rootPages.MoveToImmutable());
			}
		}
	}
}
