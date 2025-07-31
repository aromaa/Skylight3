using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigurePartEntityTypeConfiguration : IEntityTypeConfiguration<FigurePartEntity>
{
	public void Configure(EntityTypeBuilder<FigurePartEntity> builder)
	{
		builder.ToTable("figure_parts");

		builder.Property(e => e.Key)
			.HasMaxLength(64);

		builder.HasOne(e => e.PartType)
			.WithMany()
			.HasForeignKey(e => e.PartTypeId);

		builder.HasIndex(p => new { p.Key, FigurePartTypeId = p.PartTypeId })
			.IsUnique();
	}
}
