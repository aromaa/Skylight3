using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureSetEntityTypeConfiguration : IEntityTypeConfiguration<FigureSetEntity>
{
	public void Configure(EntityTypeBuilder<FigureSetEntity> builder)
	{
		builder.ToTable("figure_sets");

		builder.HasKey(s => s.Id);

		builder.HasOne(e => e.SetType)
			.WithMany(e => e.Sets)
			.HasForeignKey(e => e.SetTypeId);

		builder.HasOne(e => e.Rank)
			.WithMany()
			.HasForeignKey(e => e.RankId);

		builder.HasMany(e => e.Parts)
			.WithOne(e => e.Set)
			.HasForeignKey(e => e.SetId);

		builder.HasMany(e => e.HiddenLayers)
			.WithOne(e => e.Set)
			.HasForeignKey(e => e.SetId);

		builder.HasAlternateKey(s => new { s.Id, s.SetTypeId });
	}
}
