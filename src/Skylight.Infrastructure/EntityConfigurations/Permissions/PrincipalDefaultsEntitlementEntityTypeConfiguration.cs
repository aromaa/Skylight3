using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class PrincipalDefaultsEntitlementEntityTypeConfiguration : IEntityTypeConfiguration<PrincipalDefaultsEntitlementEntity>
{
	public void Configure(EntityTypeBuilder<PrincipalDefaultsEntitlementEntity> builder)
	{
		builder.ToTable("principal_defaults_entitlements");

		builder.HasKey(e => new { e.Identifier, e.Entitlement });
	}
}
