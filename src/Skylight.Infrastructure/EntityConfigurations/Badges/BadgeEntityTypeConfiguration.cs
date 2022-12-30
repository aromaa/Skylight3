using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Badges;

namespace Skylight.Infrastructure.EntityConfigurations.Badges;

internal sealed class BadgeEntityTypeConfiguration : IEntityTypeConfiguration<BadgeEntity>
{
	public void Configure(EntityTypeBuilder<BadgeEntity> builder)
	{
		builder.ToTable("badges");

		builder.HasKey(badge => badge.Id);

		builder.Property(b => b.Code)
			.HasMaxLength(64);

		builder.HasIndex(b => b.Code)
			.IsUnique();
	}
}
