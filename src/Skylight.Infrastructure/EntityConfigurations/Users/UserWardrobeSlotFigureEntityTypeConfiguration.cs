using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserWardrobeSlotFigureEntityTypeConfiguration : IEntityTypeConfiguration<UserWardrobeSlotFigureEntity>
{
	public void Configure(EntityTypeBuilder<UserWardrobeSlotFigureEntity> builder)
	{
		builder.ToTable("user_wardrobe_figure");

		builder.HasKey(e => new { e.UserId, e.SlotId, e.SetTypeId });

		builder.HasOne<FigureSetEntity>()
			.WithMany()
			.HasForeignKey(e => new { e.SetId, e.SetTypeId })
			.HasPrincipalKey(e => new { e.Id, e.SetTypeId });
	}
}
