using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureValidationSetTypeRuleEntityTypeConfiguration : IEntityTypeConfiguration<FigureValidationSetTypeRuleEntity>
{
	public void Configure(EntityTypeBuilder<FigureValidationSetTypeRuleEntity> builder)
	{
		builder.ToTable("figure_validation_set_type_rules");

		builder.HasKey(e => e.Id);

		builder.HasOne(e => e.Validation)
			.WithMany(e => e.SetTypeRules)
			.HasForeignKey(e => e.ValidationId);

		builder.HasOne(e => e.SetType)
			.WithMany()
			.HasForeignKey(e => e.SetTypeId);

		builder.HasMany(e => e.ExemptRanks)
			.WithMany()
			.UsingEntity<FigureValidationSetTypeRuleExemptRankEntity>(
				r => r.HasOne<RankEntity>(e => e.RankEntity).WithMany().HasForeignKey(e => e.RankId),
				l => l.HasOne<FigureValidationSetTypeRuleEntity>(e => e.SetTypeRule).WithMany().HasForeignKey(e => e.SetTypeRuleId));
	}
}
