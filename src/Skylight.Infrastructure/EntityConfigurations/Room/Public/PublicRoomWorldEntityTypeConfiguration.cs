using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Public;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Public;

internal sealed class PublicRoomWorldEntityTypeConfiguration : IEntityTypeConfiguration<PublicRoomWorldEntity>
{
	public void Configure(EntityTypeBuilder<PublicRoomWorldEntity> builder)
	{
		builder.ToTable("rooms_public_worlds");

		builder.HasKey(e => new { e.RoomId, e.WorldId });

		builder.HasOne(e => e.Room)
			.WithMany()
			.HasForeignKey(e => e.RoomId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(e => e.Layout)
			.WithMany()
			.HasForeignKey(e => e.LayoutId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
