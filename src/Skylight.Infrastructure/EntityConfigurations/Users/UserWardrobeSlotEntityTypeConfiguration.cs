using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserWardrobeSlotEntityTypeConfiguration : IEntityTypeConfiguration<UserWardrobeSlotEntity>
{
	public void Configure(EntityTypeBuilder<UserWardrobeSlotEntity> builder)
	{
		builder.ToTable("user_wardrobe_slots");

		builder.HasKey(e => new { e.UserId, e.SlotId });
	}
}
