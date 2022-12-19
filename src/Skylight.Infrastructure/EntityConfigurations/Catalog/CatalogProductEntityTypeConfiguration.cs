using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogProductEntityTypeConfiguration : IEntityTypeConfiguration<CatalogProductEntity>
{
	public void Configure(EntityTypeBuilder<CatalogProductEntity> builder)
	{
		builder.UseTpcMappingStrategy();

		builder.HasKey(p => p.Id);

		builder.HasOne(p => p.Offer)
			.WithMany(o => o.Products)
			.HasForeignKey(p => p.OfferId);
	}
}
