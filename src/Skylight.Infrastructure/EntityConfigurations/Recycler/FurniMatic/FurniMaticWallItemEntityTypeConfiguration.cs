using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticWallItemEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticWallItemEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticWallItemEntity> builder)
	{
		builder.ToTable("furnimatic_items_wall");

		builder.HasOne(i => i.Furniture)
			.WithMany()
			.HasForeignKey(i => i.FurnitureId);
	}
}
