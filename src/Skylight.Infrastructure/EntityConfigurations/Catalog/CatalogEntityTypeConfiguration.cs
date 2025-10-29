using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogEntityTypeConfiguration : IEntityTypeConfiguration<CatalogEntity>
{
	public void Configure(EntityTypeBuilder<CatalogEntity> builder)
	{
		builder.UseTpcMappingStrategy();

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Name)
			.HasMaxLength(64);

		builder.HasOne(e => e.AccessSet)
			.WithMany()
			.HasForeignKey(e => e.AccessSetId);
	}
}
