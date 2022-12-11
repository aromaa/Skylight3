namespace Skylight.Domain.Catalog;

public class CatalogOfferEntity
{
	public int Id { get; init; }

	public int PageId { get; set; }
	public CatalogPageEntity? Page { get; set; }

	public string Name { get; set; } = null!;

	public int OrderNum { get; set; }

	public int ClubRank { get; set; }

	public int CostCredits { get; set; }
	public int CostActivityPoints { get; set; }
	public int ActivityPointsType { get; set; }

	public TimeSpan RentTime { get; set; }

	public bool HasOffer { get; set; }

	public List<CatalogProductEntity>? Products { get; set; }
}
