using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class RankEntitlementEntityTypeConfiguration : IEntityTypeConfiguration<RankEntitlementEntity>
{
	public void Configure(EntityTypeBuilder<RankEntitlementEntity> builder)
	{
		builder.ToTable("rank_entitlements");

		builder.HasKey(e => new { e.RankId, e.Entitlement });
	}
}
