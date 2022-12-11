using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Items;

namespace Skylight.Infrastructure.EntityConfigurations.Items;

internal sealed class WallItemEntityTypeConfiguration : IEntityTypeConfiguration<WallItemEntity>
{
	public void Configure(EntityTypeBuilder<WallItemEntity> builder)
	{
		builder.ToTable("items_wall");

		builder.HasKey(f => f.Id);

		builder.Property(f => f.UserId)
			.IsConcurrencyToken();

		builder.Property(f => f.LocationX)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(f => f.LocationY)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(f => f.PositionX)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(f => f.PositionY)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(f => f.Direction)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.HasOne(f => f.User)
			.WithMany(u => u.WallItems)
			.HasForeignKey(f => f.UserId);

		builder.HasOne(f => f.Furniture)
			.WithMany()
			.HasForeignKey(f => f.FurnitureId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(f => f.Room)
			.WithMany()
			.HasForeignKey(f => f.RoomId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasIndex(f => f.UserId); //TODO: EFCore bug
		builder.HasIndex(f => new { f.UserId, f.RoomId }).HasFilter("room_id IS NULL");
	}
}
