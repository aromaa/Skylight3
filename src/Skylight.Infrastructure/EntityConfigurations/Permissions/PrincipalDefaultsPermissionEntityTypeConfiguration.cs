using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class PrincipalDefaultsPermissionEntityTypeConfiguration : IEntityTypeConfiguration<PrincipalDefaultsPermissionEntity>
{
	public void Configure(EntityTypeBuilder<PrincipalDefaultsPermissionEntity> builder)
	{
		builder.ToTable("principal_defaults_permissions");

		builder.HasKey(e => new { e.Identifier, e.Permission });
	}
}
