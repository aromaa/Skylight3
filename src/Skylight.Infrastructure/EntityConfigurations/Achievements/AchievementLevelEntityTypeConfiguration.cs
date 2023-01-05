using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Achievements;

namespace Skylight.Infrastructure.EntityConfigurations.Achievements;

internal sealed class AchievementLevelEntityTypeConfiguration : IEntityTypeConfiguration<AchievementLevelEntity>
{
	public void Configure(EntityTypeBuilder<AchievementLevelEntity> builder)
	{
		builder.ToTable("achievement_levels");

		builder.HasKey(a => new { a.AchievementId, a.Level });

		builder.HasOne(a => a.Achievement)
			.WithMany(a => a.Levels)
			.HasForeignKey(a => a.AchievementId);

		builder.HasOne(a => a.Badge)
			.WithMany()
			.HasForeignKey(a => a.BadgeCode)
			.HasPrincipalKey(b => b.Code);
	}
}
