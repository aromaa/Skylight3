using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogFloorProductEntityTypeConfiguration : IEntityTypeConfiguration<CatalogFloorProductEntity>
{
	public void Configure(EntityTypeBuilder<CatalogFloorProductEntity> builder)
	{
		builder.ToTable("catalog_products_floor");

		builder.HasOne(p => p.Furniture)
			.WithMany()
			.HasForeignKey(p => p.FurnitureId);
	}
}
