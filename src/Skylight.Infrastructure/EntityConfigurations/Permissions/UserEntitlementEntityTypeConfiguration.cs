using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class UserEntitlementEntityTypeConfiguration : IEntityTypeConfiguration<UserEntitlementEntity>
{
	public void Configure(EntityTypeBuilder<UserEntitlementEntity> builder)
	{
		builder.ToTable("user_entitlements");

		builder.HasKey(e => new { e.UserId, e.Entitlement });
	}
}
