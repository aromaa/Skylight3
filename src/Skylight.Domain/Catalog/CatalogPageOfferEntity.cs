using Skylight.Domain.Permissions;

namespace Skylight.Domain.Catalog;

public abstract class CatalogPageOfferEntity<TCatalog, TView, TOffer>
	where TCatalog : CatalogEntity<TCatalog, TView, TOffer>
	where TView : CatalogPageViewEntity<TCatalog, TView, TOffer>
	where TOffer : CatalogPageOfferEntity<TCatalog, TView, TOffer>
{
	public int Id { get; init; }

	public int ViewId { get; set; }
	public TView? View { get; set; }

	public int OfferId { get; set; }
	public CatalogOfferEntity? Offer { get; set; }

	public int OfferOrderNum { get; set; }
	public int PageOrderNum { get; set; }

	public string? RankId { get; set; }
	public RankEntity? Rank { get; set; }
}
