using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureColorPaletteEntityTypeConfiguration : IEntityTypeConfiguration<FigureColorPaletteEntity>
{
	public void Configure(EntityTypeBuilder<FigureColorPaletteEntity> builder)
	{
		builder.ToTable("figure_color_palettes");

		builder.HasKey(p => p.Id);

		builder.HasMany(e => e.Colors)
			.WithOne(e => e.Palette)
			.HasForeignKey(e => e.PaletteId);
	}
}
