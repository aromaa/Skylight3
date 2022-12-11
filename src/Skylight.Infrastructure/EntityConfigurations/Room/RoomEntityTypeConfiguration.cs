using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms;

namespace Skylight.Infrastructure.EntityConfigurations.Room;

internal sealed class RoomEntityTypeConfiguration : IEntityTypeConfiguration<RoomEntity>
{
	public void Configure(EntityTypeBuilder<RoomEntity> builder)
	{
		builder.ToTable("rooms");

		builder.HasKey(r => r.Id);

		builder.Property(r => r.Name)
			.HasMaxLength(25);

		builder.Property(r => r.Description)
			.HasMaxLength(128)
			.HasDefaultValue(string.Empty);

		builder.Property(r => r.LayoutId)
			.HasDefaultValue("model_a");

		builder.Property(r => r.CategoryId)
			.HasDefaultValue(1);

		builder.Property(r => r.UsersMax)
			.HasDefaultValue(25);

		builder.HasOne(r => r.Owner)
			.WithMany(u => u.Rooms)
			.HasForeignKey(r => r.OwnerId);

		builder.HasOne(r => r.Layout)
			.WithMany()
			.HasForeignKey(r => r.LayoutId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(r => r.Category)
			.WithMany()
			.HasForeignKey(r => r.CategoryId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
