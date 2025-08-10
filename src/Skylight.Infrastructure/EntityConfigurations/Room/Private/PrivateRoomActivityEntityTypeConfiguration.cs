using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Private;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Private;

internal sealed class PrivateRoomActivityEntityTypeConfiguration : IEntityTypeConfiguration<PrivateRoomActivityEntity>
{
	public void Configure(EntityTypeBuilder<PrivateRoomActivityEntity> builder)
	{
		builder.ToTable("rooms_private_activity");

		builder.HasKey(e => new { e.RoomId, e.Day });
	}
}
