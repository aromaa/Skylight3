using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureSetPartEntityTypeConfiguration : IEntityTypeConfiguration<FigureSetPartEntity>
{
	public void Configure(EntityTypeBuilder<FigureSetPartEntity> builder)
	{
		builder.ToTable("figure_set_parts");

		builder.HasOne(e => e.Set)
			.WithMany(e => e.Parts)
			.HasForeignKey(e => e.SetId);

		builder.HasOne(e => e.Part)
			.WithMany()
			.HasForeignKey(e => e.PartId);

		builder.HasKey(e => new { e.SetId, e.PartId, e.OrderNum });
	}
}
