using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class LeaseCatalogPageOfferEntityTypeConfiguration : CatalogPageOfferEntityTypeConfiguration<LeaseCatalogEntity, LeaseCatalogPageViewEntity, LeaseCatalogPageOfferEntity>
{
	public override void Configure(EntityTypeBuilder<LeaseCatalogPageOfferEntity> builder)
	{
		builder.ToTable("catalog_page_offers_lease");

		base.Configure(builder);
	}
}
