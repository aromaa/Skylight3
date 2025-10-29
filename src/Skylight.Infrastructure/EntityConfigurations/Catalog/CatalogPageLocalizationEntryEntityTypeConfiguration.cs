using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogPageLocalizationEntryEntityTypeConfiguration : IEntityTypeConfiguration<CatalogPageLocalizationEntryEntity>
{
	public void Configure(EntityTypeBuilder<CatalogPageLocalizationEntryEntity> builder)
	{
		builder.ToTable("catalog_page_localization_entries");

		builder.HasKey(e => new { e.LocalizationId, e.Locale });

		builder.Property(e => e.Name)
			.HasMaxLength(64);

		builder.Property(e => e.Texts)
			.HasDefaultValue(new List<string>());

		builder.Property(e => e.Images)
			.HasDefaultValue(new List<string>());

		builder.HasOne(e => e.Localization)
			.WithMany(e => e.Entries)
			.HasForeignKey(e => e.LocalizationId);
	}
}
