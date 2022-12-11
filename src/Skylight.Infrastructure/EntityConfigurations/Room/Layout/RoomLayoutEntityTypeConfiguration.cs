using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Layout;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Layout;

internal sealed class RoomLayoutEntityTypeConfiguration : IEntityTypeConfiguration<RoomLayoutEntity>
{
	public void Configure(EntityTypeBuilder<RoomLayoutEntity> builder)
	{
		builder.ToTable("room_layouts");

		builder.HasKey(l => l.Id);

		builder.Property(l => l.Id)
			.HasMaxLength(16);

		builder.Property(u => u.DoorDirection)
			.HasDefaultValue(2)
			.ValueGeneratedNever();
	}
}
