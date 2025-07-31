using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureSetHiddenLayerEntityTypeConfiguration : IEntityTypeConfiguration<FigureSetHiddenLayerEntity>
{
	public void Configure(EntityTypeBuilder<FigureSetHiddenLayerEntity> builder)
	{
		builder.ToTable("figure_set_hidden_layers");

		builder.HasOne(e => e.Set)
			.WithMany(e => e.HiddenLayers)
			.HasForeignKey(e => e.SetId);

		builder.HasOne(e => e.PartType)
			.WithMany()
			.HasForeignKey(e => e.PartTypeId);

		builder.HasKey(e => new { FigureSetId = e.SetId, FigurePartTypeId = e.PartTypeId });
	}
}
