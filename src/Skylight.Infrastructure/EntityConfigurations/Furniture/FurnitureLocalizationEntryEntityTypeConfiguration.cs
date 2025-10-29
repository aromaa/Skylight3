using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal sealed class FurnitureLocalizationEntryEntityTypeConfiguration : IEntityTypeConfiguration<FurnitureLocalizationEntryEntity>
{
	public void Configure(EntityTypeBuilder<FurnitureLocalizationEntryEntity> builder)
	{
		builder.ToTable("furniture_localization_entries");

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
