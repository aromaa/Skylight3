using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Badges;

namespace Skylight.Infrastructure.EntityConfigurations.Badges;

internal sealed class UserBadgeEntityTypeConfiguration : IEntityTypeConfiguration<UserBadgeEntity>
{
	public void Configure(EntityTypeBuilder<UserBadgeEntity> builder)
	{
		builder.ToTable("user_badges");

		builder.HasKey(x => new { x.UserId, x.BadgeCode });

		builder.HasOne(b => b.Badge)
			.WithMany()
			.HasForeignKey(b => b.BadgeCode)
			.HasPrincipalKey(b => b.Code);
	}
}
