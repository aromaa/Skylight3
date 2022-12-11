using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogProductEntityTypeConfiguration : IEntityTypeConfiguration<CatalogProductEntity>
{
	public void Configure(EntityTypeBuilder<CatalogProductEntity> builder)
	{
		builder.ToTable("catalog_products");

		builder.HasKey(p => p.Id);

		builder.Property(p => p.Amount)
			.HasDefaultValue(1);

		builder.Property(p => p.ExtraData)
			.HasDefaultValue(string.Empty);

		builder.HasOne(p => p.Offer)
			.WithMany(o => o.Products)
			.HasForeignKey(p => p.OfferId);

		builder.HasOne(p => p.FloorFurniture)
			.WithMany()
			.HasForeignKey(p => p.FloorFurnitureId);

		builder.HasOne(p => p.WallFurniture)
			.WithMany()
			.HasForeignKey(p => p.WallFurnitureId);
	}
}
