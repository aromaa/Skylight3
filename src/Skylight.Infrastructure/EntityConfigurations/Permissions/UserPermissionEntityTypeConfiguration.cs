using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class UserPermissionEntityTypeConfiguration : IEntityTypeConfiguration<UserPermissionEntity>
{
	public void Configure(EntityTypeBuilder<UserPermissionEntity> builder)
	{
		builder.ToTable("user_permissions");

		builder.HasKey(e => new { e.UserId, e.Permission });
	}
}
