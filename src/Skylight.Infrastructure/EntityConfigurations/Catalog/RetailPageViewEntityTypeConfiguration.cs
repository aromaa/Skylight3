using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class RetailPageViewEntityTypeConfiguration : CatalogPageViewEntityTypeConfiguration<RetailCatalogEntity, RetailCatalogPageViewEntity, RetailCatalogPageOfferEntity>
{
	public override void Configure(EntityTypeBuilder<RetailCatalogPageViewEntity> builder)
	{
		builder.ToTable("catalog_page_views_retail");

		base.Configure(builder);
	}
}
