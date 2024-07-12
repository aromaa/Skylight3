using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Layout;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Layout;

internal sealed class CustomRoomLayoutEntityTypeConfiguration : IEntityTypeConfiguration<CustomRoomLayoutEntity>
{
	public void Configure(EntityTypeBuilder<CustomRoomLayoutEntity> builder)
	{
		builder.ToTable("room_layouts_custom");

		builder.HasKey(e => e.RoomId);

		builder.HasOne(e => e.Room)
			.WithOne()
			.HasForeignKey<CustomRoomLayoutEntity>(e => e.RoomId);
	}
}
