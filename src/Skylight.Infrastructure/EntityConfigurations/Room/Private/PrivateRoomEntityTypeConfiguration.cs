using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Private;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Private;

internal sealed class PrivateRoomEntityTypeConfiguration : IEntityTypeConfiguration<PrivateRoomEntity>
{
	public void Configure(EntityTypeBuilder<PrivateRoomEntity> builder)
	{
		builder.ToTable("rooms_private");

		builder.HasKey(r => r.Id);

		builder.Property(r => r.LayoutId)
			.HasDefaultValue("model_a");

		builder.Property(r => r.Name)
			.HasMaxLength(25);

		builder.Property(r => r.Description)
			.HasMaxLength(128)
			.HasDefaultValue(string.Empty);

		builder.Property(r => r.CategoryId)
			.HasDefaultValue(1);

		builder.Property(r => r.Tags)
			.HasDefaultValue(Array.Empty<string>());

		builder.Property(r => r.EntryMode)
			.HasDefaultValue(PrivateRoomEntryMode.Open);

		builder.Property(r => r.UsersMax)
			.HasDefaultValue(25);

		builder.Property(r => r.TradeMode)
			.HasDefaultValue(PrivateRoomTradeMode.None);

		builder.Property(r => r.WalkThrough)
			.HasDefaultValue(true);

		builder.Property(r => r.AllowPets)
			.HasDefaultValue(false);

		builder.Property(r => r.AllowPetsToEat)
			.HasDefaultValue(false);

		builder.Property(r => r.HideWalls)
			.HasDefaultValue(false);

		builder.Property(r => r.FloorThickness)
			.HasDefaultValue(0);

		builder.Property(r => r.WallThickness)
			.HasDefaultValue(0);

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
