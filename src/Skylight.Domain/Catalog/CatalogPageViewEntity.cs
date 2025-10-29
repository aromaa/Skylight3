using Skylight.Domain.Permissions;

namespace Skylight.Domain.Catalog;

public abstract class CatalogPageViewEntity<TCatalog, TView, TOffer>
	where TCatalog : CatalogEntity<TCatalog, TView, TOffer>
	where TView : CatalogPageViewEntity<TCatalog, TView, TOffer>
	where TOffer : CatalogPageOfferEntity<TCatalog, TView, TOffer>
{
	public int Id { get; init; }

	public int CatalogId { get; init; }
	public TCatalog? Catalog { get; set; }

	public int? ParentId { get; set; }
	public TView? Parent { get; set; }

	public int PageId { get; set; }
	public CatalogPageEntity? Page { get; set; }

	public int? AccessSetId { get; set; }
	public AccessSetEntity? AccessSet { get; set; }

	public int OrderNum { get; set; }

	public CatalogPageVisiblity Visiblity { get; set; }

	public List<TView>? Children { get; set; }
	public List<TOffer>? Offers { get; set; }
}
