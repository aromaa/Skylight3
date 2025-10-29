using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogOfferLocalizationEntryEntityTypeConfiguration : IEntityTypeConfiguration<CatalogOfferLocalizationEntryEntity>
{
	public void Configure(EntityTypeBuilder<CatalogOfferLocalizationEntryEntity> builder)
	{
		builder.ToTable("catalog_offer_localization_entries");

		builder.HasKey(e => new { e.LocalizationId, e.Locale });

		builder.Property(e => e.Locale)
			.HasMaxLength(8);

		builder.Property(e => e.Name)
			.HasMaxLength(1024);

		builder.Property(e => e.Description)
			.HasMaxLength(1024);

		builder.HasOne(e => e.Localization)
			.WithMany(e => e.Entries)
			.HasForeignKey(e => e.LocalizationId);
	}
}
