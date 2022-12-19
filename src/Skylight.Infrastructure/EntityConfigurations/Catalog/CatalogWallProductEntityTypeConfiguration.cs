using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogWallProductEntityTypeConfiguration : IEntityTypeConfiguration<CatalogWallProductEntity>
{
	public void Configure(EntityTypeBuilder<CatalogWallProductEntity> builder)
	{
		builder.ToTable("catalog_products_wall");

		builder.Property(p => p.Amount)
			.HasDefaultValue(1);

		builder.Property(p => p.ExtraData)
			.HasDefaultValue(string.Empty);

		builder.HasOne(p => p.Furniture)
			.WithMany()
			.HasForeignKey(p => p.FurnitureId);
	}
}
