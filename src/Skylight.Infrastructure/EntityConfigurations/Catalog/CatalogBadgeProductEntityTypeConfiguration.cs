using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogBadgeProductEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBadgeProductEntity>
{
	public void Configure(EntityTypeBuilder<CatalogBadgeProductEntity> builder)
	{
		builder.ToTable("catalog_products_badge");

		builder.Property(p => p.BadgeCode)
			.HasMaxLength(64);
	}
}
