using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticFloorItemEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticFloorItemEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticFloorItemEntity> builder)
	{
		builder.ToTable("furnimatic_items_floor");

		builder.HasOne(i => i.Furniture)
			.WithMany()
			.HasForeignKey(i => i.FurnitureId);
	}
}
