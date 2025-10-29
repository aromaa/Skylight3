using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;
using Skylight.Infrastructure.Extensions;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class RetailCatalogOfferCostEntityTypeConfiguration : IEntityTypeConfiguration<RetailCatalogOfferCostEntity>
{
	public void Configure(EntityTypeBuilder<RetailCatalogOfferCostEntity> builder)
	{
		builder.ToTable("catalog_page_offers_retail_costs");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.CurrencyType)
			.HasMaxLength(64)
			.AddCheckConstraint(c => $"{c} LIKE '%_:_%'"); // Ambiguous with unspecified namespace

		builder.Property(c => c.CurrencyData)
			.HasColumnType("jsonb");

		builder.HasOne(c => c.Offer)
			.WithMany(o => o.Cost)
			.HasForeignKey(c => c.OfferId);

		builder.HasIndex(c => new { c.OfferId, c.CurrencyType, c.CurrencyData })
			.IsUnique()
			.AreNullsDistinct();

		// EXCLUDE USING gist(currency_type WITH =, (currency_data IS NULL) WITH <>)
	}
}
