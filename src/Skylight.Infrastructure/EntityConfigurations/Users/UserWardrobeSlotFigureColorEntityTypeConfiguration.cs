using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserWardrobeSlotFigureColorEntityTypeConfiguration : IEntityTypeConfiguration<UserWardrobeSlotFigureColorEntity>
{
	public void Configure(EntityTypeBuilder<UserWardrobeSlotFigureColorEntity> builder)
	{
		builder.ToTable("user_wardrobe_figure_colors");

		builder.HasKey(e => new { e.UserId, e.SlotId, e.SetTypeId, e.Index });
	}
}
