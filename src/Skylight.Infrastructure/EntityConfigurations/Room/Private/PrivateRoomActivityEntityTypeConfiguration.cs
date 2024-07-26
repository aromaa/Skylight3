using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Private;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Private;

internal sealed class PrivateRoomActivityEntityTypeConfiguration : IEntityTypeConfiguration<PrivateRoomActivityEntity>
{
	public void Configure(EntityTypeBuilder<PrivateRoomActivityEntity> builder)
	{
		builder.ToTable("rooms_private_activity");

		builder.HasKey(e => new { e.RoomId, e.Week });

		builder.Property(e => e.Total)
			.HasComputedColumnSql(sql: "monday + tuesday + wednesday + thursday + friday + saturday + sunday", stored: true);

		builder.Property(e => e.Monday).HasDefaultValue(0);
		builder.Property(e => e.Tuesday).HasDefaultValue(0);
		builder.Property(e => e.Wednesday).HasDefaultValue(0);
		builder.Property(e => e.Thursday).HasDefaultValue(0);
		builder.Property(e => e.Friday).HasDefaultValue(0);
		builder.Property(e => e.Saturday).HasDefaultValue(0);
		builder.Property(e => e.Sunday).HasDefaultValue(0);
	}
}
