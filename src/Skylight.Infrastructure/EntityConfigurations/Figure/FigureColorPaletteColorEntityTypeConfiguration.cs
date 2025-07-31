using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureColorPaletteColorEntityTypeConfiguration : IEntityTypeConfiguration<FigureColorPaletteColorEntity>
{
	public void Configure(EntityTypeBuilder<FigureColorPaletteColorEntity> builder)
	{
		builder.ToTable("figure_color_palette_colors");

		builder.HasKey(c => c.Id);

		builder.HasOne(e => e.Palette)
			.WithMany(e => e.Colors)
			.HasForeignKey(e => e.PaletteId);

		builder.HasOne(e => e.Rank)
			.WithMany()
			.HasForeignKey(e => e.RankId);
	}
}
