using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserWardrobeEntityTypeConfiguration : IEntityTypeConfiguration<UserWardrobeSlotEntity>
{
	public void Configure(EntityTypeBuilder<UserWardrobeSlotEntity> builder)
	{
		builder.ToTable("user_wardrobe");

		builder.HasKey(e => new { e.UserId, Slot = e.SlotId });

		builder.Property(u => u.Gender)
			.HasMaxLength(1);

		builder.Property(u => u.Figure)
			.HasMaxLength(128);
	}
}
