using Skylight.Domain.Badges;

namespace Skylight.Domain.Catalog;

public class CatalogBadgeProductEntity : CatalogProductEntity
{
	public string BadgeCode { get; set; } = null!;
	public BadgeEntity? Badge { get; set; }
}
