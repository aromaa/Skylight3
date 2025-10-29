using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class AccessSetRuleEntityTypeConfiguration : IEntityTypeConfiguration<AccessSetRuleEntity>
{
	public void Configure(EntityTypeBuilder<AccessSetRuleEntity> builder)
	{
		builder.UseTpcMappingStrategy();

		builder.HasKey(e => e.Id);

		builder.HasOne(e => e.Set)
			.WithMany(e => e.Rules)
			.HasForeignKey(e => e.SetId);
	}
}
