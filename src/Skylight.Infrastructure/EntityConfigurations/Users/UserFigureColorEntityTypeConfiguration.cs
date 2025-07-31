using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserFigureColorEntityTypeConfiguration : IEntityTypeConfiguration<UserFigureColorEntity>
{
	public void Configure(EntityTypeBuilder<UserFigureColorEntity> builder)
	{
		builder.ToTable("user_figure_colors");

		builder.HasKey(e => new { e.UserId, e.SetTypeId, e.Index });

		builder.HasOne<UserFigureEntity>()
			.WithMany(e => e.Colors)
			.HasForeignKey(e => new { e.UserId, e.SetTypeId });
	}
}
