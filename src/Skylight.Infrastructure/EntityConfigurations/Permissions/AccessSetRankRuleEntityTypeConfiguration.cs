using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class AccessSetRankRuleEntityTypeConfiguration : IEntityTypeConfiguration<AccessSetRankRuleEntity>
{
	public void Configure(EntityTypeBuilder<AccessSetRankRuleEntity> builder)
	{
		builder.ToTable("access_set_rules_rank");
	}
}
