using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;
using Skylight.Infrastructure.Extensions;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogOfferCostEntityTypeConfiguration : IEntityTypeConfiguration<CatalogOfferCostEntity>
{
	public void Configure(EntityTypeBuilder<CatalogOfferCostEntity> builder)
	{
		builder.ToTable("catalog_offer_costs");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.CurrencyType)
			.AddCheckConstraint(c => $"{c} LIKE '%_:_%'"); // Ambiguous without a valid key

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
