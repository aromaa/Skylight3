using Skylight.Domain.Permissions;

namespace Skylight.Domain.Catalog;

public class CatalogOfferEntity
{
	public int Id { get; init; }

	public int PageId { get; set; }
	public CatalogPageEntity? Page { get; set; }

	public int OrderNum { get; set; }

	public string Name { get; set; } = null!;

	public string? RankId { get; set; }
	public RankEntity? Rank { get; set; }

	public TimeSpan RentTime { get; set; }

	public bool HasOffer { get; set; }

	public List<CatalogOfferCostEntity>? Cost { get; set; }
	public List<CatalogProductEntity>? Products { get; set; }
}
