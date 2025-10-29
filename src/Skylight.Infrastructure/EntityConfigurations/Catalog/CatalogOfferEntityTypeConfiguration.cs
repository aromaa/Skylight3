using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogOfferEntityTypeConfiguration : IEntityTypeConfiguration<CatalogOfferEntity>
{
	public void Configure(EntityTypeBuilder<CatalogOfferEntity> builder)
	{
		builder.ToTable("catalog_offers");

		builder.HasKey(e => e.Id);

		builder.HasOne(e => e.Localization)
			.WithMany()
			.HasForeignKey(e => e.LocalizationId);

		builder.HasMany(e => e.Products)
			.WithOne()
			.HasForeignKey(e => e.OfferId);
	}
}
