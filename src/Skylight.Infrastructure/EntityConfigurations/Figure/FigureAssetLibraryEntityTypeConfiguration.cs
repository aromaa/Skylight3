using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureAssetLibraryEntityTypeConfiguration : IEntityTypeConfiguration<FigureAssetLibraryEntity>
{
	public void Configure(EntityTypeBuilder<FigureAssetLibraryEntity> builder)
	{
		builder.ToTable("figure_asset_libraries");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.FileName)
			.HasMaxLength(128);

		builder.Property(e => e.Revision)
			.HasDefaultValue(0);

		builder.HasMany(e => e.AssetParts)
			.WithOne(e => e.AssetLibrary)
			.HasForeignKey(e => e.AssetLibraryId);

		builder.HasIndex(e => e.FileName)
			.IsUnique();
	}
}
