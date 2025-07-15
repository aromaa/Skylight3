using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class UserRankEntityTypeConfiguration : IEntityTypeConfiguration<UserRankEntity>
{
	public void Configure(EntityTypeBuilder<UserRankEntity> builder)
	{
		builder.ToTable("user_ranks");

		builder.HasKey(e => new { e.UserId, e.RankId });
	}
}
