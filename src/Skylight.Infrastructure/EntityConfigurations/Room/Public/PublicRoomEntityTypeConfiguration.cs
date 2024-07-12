using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Public;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Public;

internal sealed class PublicRoomEntityTypeConfiguration : IEntityTypeConfiguration<PublicRoomEntity>
{
	public void Configure(EntityTypeBuilder<PublicRoomEntity> builder)
	{
		builder.ToTable("rooms_public");

		builder.HasKey(e => e.Id);
	}
}
