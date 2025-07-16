namespace Skylight.Domain.Catalog;

public class CatalogPageEntity
{
	public int Id { get; init; }

	public int? ParentId { get; set; }
	public CatalogPageEntity? Parent { get; set; }

	public string Type { get; set; } = null!;

	public string Name { get; set; } = null!;
	public string Localization { get; set; } = null!;

	public int OrderNum { get; set; }

	public bool Enabled { get; set; }
	public bool Visible { get; set; }

	public int IconColor { get; set; }
	public int IconImage { get; set; }

	public string Layout { get; set; } = null!;

	public List<string> Texts { get; set; } = null!;
	public List<string> Images { get; set; } = null!;

	public bool AcceptSeasonCurrencyAsCredits { get; set; }

	public List<CatalogPageAccessEntity>? Access { get; set; }
	public List<CatalogPageEntity>? Children { get; set; }
	public List<CatalogOfferEntity>? Offers { get; set; }
}
