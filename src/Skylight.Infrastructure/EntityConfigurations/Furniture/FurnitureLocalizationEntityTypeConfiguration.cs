using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal sealed class FurnitureLocalizationEntityTypeConfiguration : IEntityTypeConfiguration<FurnitureLocalizationEntity>
{
	public void Configure(EntityTypeBuilder<FurnitureLocalizationEntity> builder)
	{
		builder.ToTable("furniture_localizations");

		builder.HasKey(e => e.Id);

		builder.HasMany(e => e.Entries)
			.WithOne(e => e.Localization)
			.HasForeignKey(e => e.LocalizationId);
	}
}
