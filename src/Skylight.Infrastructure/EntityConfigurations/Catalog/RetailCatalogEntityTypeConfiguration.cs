using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class RetailCatalogEntityTypeConfiguration : IEntityTypeConfiguration<RetailCatalogEntity>
{
	public void Configure(EntityTypeBuilder<RetailCatalogEntity> builder)
	{
		builder.ToTable("catalogs_retail");
	}
}
