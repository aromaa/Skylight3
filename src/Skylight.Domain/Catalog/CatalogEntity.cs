using Skylight.Domain.Permissions;

namespace Skylight.Domain.Catalog;

public abstract class CatalogEntity
{
	public int Id { get; init; }

	public string Name { get; set; } = null!;

	public int? AccessSetId { get; set; }
	public AccessSetEntity? AccessSet { get; set; }
}

public abstract class CatalogEntity<TCatalog, TView, TOffer> : CatalogEntity
	where TCatalog : CatalogEntity<TCatalog, TView, TOffer>
	where TView : CatalogPageViewEntity<TCatalog, TView, TOffer>
	where TOffer : CatalogPageOfferEntity<TCatalog, TView, TOffer>
{
	public List<TView>? Views { get; set; }
}
