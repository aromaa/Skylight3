using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticItemEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticItemEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticItemEntity> builder)
	{
		builder.ToTable("furnimatic_items");

		builder.HasKey(i => i.Id);

		builder.HasOne(i => i.Prize)
			.WithMany(p => p.Items)
			.HasForeignKey(i => i.PrizeId);

		builder.HasOne(i => i.FloorFurniture)
			.WithMany()
			.HasForeignKey(i => i.FloorFurnitureId);

		builder.HasOne(i => i.WallFurniture)
			.WithMany()
			.HasForeignKey(i => i.WallFurnitureId);
	}
}
