using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureValidationSetTypeRuleExemptRankEntityTypeConfiguration : IEntityTypeConfiguration<FigureValidationSetTypeRuleExemptRankEntity>
{
	public void Configure(EntityTypeBuilder<FigureValidationSetTypeRuleExemptRankEntity> builder)
	{
		builder.ToTable("figure_validation_set_type_rule_exempt_ranks");

		builder.HasKey(e => new { e.SetTypeRuleId, e.RankId });

		builder.HasOne(e => e.RankEntity)
			.WithMany()
			.HasForeignKey(e => e.RankId);

		builder.HasOne(e => e.SetTypeRule)
			.WithMany();
	}
}
