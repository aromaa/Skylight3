using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticGiftEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticGiftEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticGiftEntity> builder)
	{
		builder.ToTable("items_furnimatic");

		builder.HasKey(i => i.ItemId);

		builder.HasOne(i => i.Item)
			.WithOne()
			.HasForeignKey<FurniMaticGiftEntity>(i => i.ItemId);

		builder.HasOne(i => i.Prize)
			.WithOne()
			.HasForeignKey<FurniMaticGiftEntity>(i => i.PrizeId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
