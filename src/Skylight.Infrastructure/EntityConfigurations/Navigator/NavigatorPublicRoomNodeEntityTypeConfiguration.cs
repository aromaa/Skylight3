using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Navigator;

namespace Skylight.Infrastructure.EntityConfigurations.Navigator;

internal sealed class NavigatorPublicRoomNodeEntityTypeConfiguration : IEntityTypeConfiguration<NavigatorPublicRoomNodeEntity>
{
	public void Configure(EntityTypeBuilder<NavigatorPublicRoomNodeEntity> builder)
	{
		builder.HasOne(e => e.PublicRoomWorld)
			.WithMany()
			.HasForeignKey(e => new { e.RoomId, e.WorldId })
			.OnDelete(DeleteBehavior.NoAction);
	}
}
