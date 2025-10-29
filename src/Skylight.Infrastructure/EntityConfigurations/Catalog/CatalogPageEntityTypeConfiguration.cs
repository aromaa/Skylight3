using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogPageEntityTypeConfiguration : IEntityTypeConfiguration<CatalogPageEntity>
{
	public void Configure(EntityTypeBuilder<CatalogPageEntity> builder)
	{
		builder.ToTable("catalog_pages");
		builder.ToTable(t => t.HasCheckConstraint("ck_catalog_pages_id_range", "id >= 0"));

		builder.HasKey(p => p.Id);

		builder.Property(p => p.IconColor)
			.HasDefaultValue(1);

		builder.Property(p => p.IconImage)
			.HasDefaultValue(1);

		builder.Property(p => p.Layout)
			.HasDefaultValue("default_3x3")
			.HasMaxLength(32);

		builder.HasOne(e => e.Localization)
			.WithMany()
			.HasForeignKey(e => e.LocalizationId);
	}
}
