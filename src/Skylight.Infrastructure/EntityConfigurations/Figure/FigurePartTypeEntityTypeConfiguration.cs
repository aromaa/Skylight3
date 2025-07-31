using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigurePartTypeEntityTypeConfiguration : IEntityTypeConfiguration<FigurePartTypeEntity>
{
	public void Configure(EntityTypeBuilder<FigurePartTypeEntity> builder)
	{
		builder.ToTable("figure_part_types");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Type)
			.HasMaxLength(64);
	}
}
