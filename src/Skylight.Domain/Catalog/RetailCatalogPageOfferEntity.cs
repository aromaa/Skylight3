namespace Skylight.Domain.Catalog;

public class RetailCatalogPageOfferEntity : CatalogPageOfferEntity<RetailCatalogEntity, RetailCatalogPageViewEntity, RetailCatalogPageOfferEntity>
{
	public TimeSpan RentTime { get; set; }

	public bool BulkDiscount { get; set; }

	public List<RetailCatalogOfferCostEntity>? Cost { get; set; }
}
