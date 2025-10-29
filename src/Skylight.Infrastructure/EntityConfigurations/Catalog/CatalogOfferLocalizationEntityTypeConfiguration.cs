using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogOfferLocalizationEntityTypeConfiguration : IEntityTypeConfiguration<CatalogOfferLocalizationEntity>
{
	public void Configure(EntityTypeBuilder<CatalogOfferLocalizationEntity> builder)
	{
		builder.ToTable("catalog_offer_localizations");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Code)
			.HasMaxLength(64);

		builder.HasAlternateKey(e => e.Code);

		builder.HasMany(e => e.Entries)
			.WithOne(e => e.Localization)
			.HasForeignKey(e => e.LocalizationId);
	}
}
