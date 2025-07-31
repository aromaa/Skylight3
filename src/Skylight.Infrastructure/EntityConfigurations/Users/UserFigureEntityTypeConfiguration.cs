using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserFigureEntityTypeConfiguration : IEntityTypeConfiguration<UserFigureEntity>
{
	public void Configure(EntityTypeBuilder<UserFigureEntity> builder)
	{
		builder.ToTable("user_figure");

		builder.HasKey(e => new { e.UserId, e.SetTypeId });

		builder.HasOne<FigureSetEntity>()
			.WithMany()
			.HasForeignKey(e => new { e.SetId, e.SetTypeId })
			.HasPrincipalKey(e => new { e.Id, e.SetTypeId });
	}
}
