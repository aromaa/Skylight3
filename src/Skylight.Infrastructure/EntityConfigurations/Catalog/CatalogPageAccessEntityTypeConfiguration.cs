using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogPageAccessEntityTypeConfiguration : IEntityTypeConfiguration<CatalogPageAccessEntity>
{
	public void Configure(EntityTypeBuilder<CatalogPageAccessEntity> builder)
	{
		builder.ToTable("catalog_page_access");

		builder.HasKey(x => x.Id);

		builder.HasOne(e => e.Page)
			.WithMany(e => e.Access)
			.HasForeignKey(e => e.PageId);
	}
}
