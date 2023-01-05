using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Achievements;

namespace Skylight.Infrastructure.EntityConfigurations.Achievements;

internal sealed class AchievementEntityTypeConfiguration : IEntityTypeConfiguration<AchievementEntity>
{
	public void Configure(EntityTypeBuilder<AchievementEntity> builder)
	{
		builder.ToTable("achievements");

		builder.HasKey(a => a.Id);

		builder.Property(a => a.Category)
			.HasMaxLength(32);
	}
}
