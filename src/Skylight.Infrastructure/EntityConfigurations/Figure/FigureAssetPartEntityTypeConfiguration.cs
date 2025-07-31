using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureAssetPartEntityTypeConfiguration : IEntityTypeConfiguration<FigureAssetPartEntity>
{
	public void Configure(EntityTypeBuilder<FigureAssetPartEntity> builder)
	{
		builder.ToTable("figure_asset_parts");

		builder.HasKey(e => e.Id);

		builder.HasOne(e => e.AssetLibrary)
			.WithMany(e => e.AssetParts)
			.HasForeignKey(e => e.AssetLibraryId);

		builder.HasOne(e => e.Part);
	}
}
