using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class LeaseCatalogEntityTypeConfiguration : IEntityTypeConfiguration<LeaseCatalogEntity>
{
	public void Configure(EntityTypeBuilder<LeaseCatalogEntity> builder)
	{
		builder.ToTable("catalogs_lease");
	}
}
