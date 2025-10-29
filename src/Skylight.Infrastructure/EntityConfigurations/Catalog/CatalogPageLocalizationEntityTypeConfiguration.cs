using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogPageLocalizationEntityTypeConfiguration : IEntityTypeConfiguration<CatalogPageLocalizationEntity>
{
	public void Configure(EntityTypeBuilder<CatalogPageLocalizationEntity> builder)
	{
		builder.ToTable("catalog_page_localizations");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Code)
			.HasMaxLength(64);

		builder.HasAlternateKey(e => e.Code);

		builder.HasMany(e => e.Entries)
			.WithOne(e => e.Localization)
			.HasForeignKey(e => e.LocalizationId);
	}
}
