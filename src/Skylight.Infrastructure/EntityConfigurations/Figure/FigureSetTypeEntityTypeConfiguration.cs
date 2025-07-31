using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureSetTypeEntityTypeConfiguration : IEntityTypeConfiguration<FigureSetTypeEntity>
{
	public void Configure(EntityTypeBuilder<FigureSetTypeEntity> builder)
	{
		builder.ToTable("figure_set_types");

		builder.HasKey(s => s.Id);

		builder.Property(e => e.Type)
			.HasMaxLength(64);

		builder.HasOne(e => e.ColorPalette)
			.WithMany()
			.HasForeignKey(e => e.ColorPaletteId);

		builder.HasMany(e => e.Sets)
			.WithOne(e => e.SetType)
			.HasForeignKey(e => e.SetTypeId);

		builder.HasIndex(e => e.Type)
			.IsUnique();
	}
}
