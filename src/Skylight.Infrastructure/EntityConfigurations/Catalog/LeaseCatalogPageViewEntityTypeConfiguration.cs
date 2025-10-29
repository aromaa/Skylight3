using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class LeaseCatalogPageViewEntityTypeConfiguration : CatalogPageViewEntityTypeConfiguration<LeaseCatalogEntity, LeaseCatalogPageViewEntity, LeaseCatalogPageOfferEntity>
{
	public override void Configure(EntityTypeBuilder<LeaseCatalogPageViewEntity> builder)
	{
		builder.ToTable("catalog_page_views_lease");

		base.Configure(builder);
	}
}
