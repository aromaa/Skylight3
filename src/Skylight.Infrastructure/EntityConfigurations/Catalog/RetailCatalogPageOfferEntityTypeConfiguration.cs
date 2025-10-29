using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class RetailCatalogPageOfferEntityTypeConfiguration : CatalogPageOfferEntityTypeConfiguration<RetailCatalogEntity, RetailCatalogPageViewEntity, RetailCatalogPageOfferEntity>
{
	public override void Configure(EntityTypeBuilder<RetailCatalogPageOfferEntity> builder)
	{
		builder.ToTable("catalog_page_offers_retail");

		builder.Property(o => o.RentTime)
			.HasDefaultValue(TimeSpan.Zero)
			.ValueGeneratedNever();

		builder.Property(o => o.BulkDiscount)
			.HasDefaultValue(false)
			.ValueGeneratedNever();

		base.Configure(builder);
	}
}
