using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class RankPermissionEntityTypeConfiguration : IEntityTypeConfiguration<RankPermissionEntity>
{
	public void Configure(EntityTypeBuilder<RankPermissionEntity> builder)
	{
		builder.ToTable("rank_permissions");

		builder.HasKey(e => new { e.RankId, e.Permission });
	}
}
