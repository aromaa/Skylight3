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

		builder.Property(p => p.Type)
			.HasMaxLength(32)
			.HasDefaultValue("NORMAL");

		builder.Property(p => p.Name)
			.HasMaxLength(32);

		builder.Property(p => p.Localization)
			.HasMaxLength(32);

		builder.Property(p => p.OrderNum)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(p => p.Enabled)
			.HasDefaultValue(true)
			.ValueGeneratedNever();

		builder.Property(p => p.Visible)
			.HasDefaultValue(true)
			.ValueGeneratedNever();

		builder.Property(p => p.IconColor)
			.HasDefaultValue(1);

		builder.Property(p => p.IconImage)
			.HasDefaultValue(1);

		builder.Property(p => p.Layout)
			.HasDefaultValue("default_3x3");

		builder.Property(p => p.Texts)
			.HasDefaultValue(new List<string>());

		builder.Property(p => p.Images)
			.HasDefaultValue(new List<string>());

		builder.HasOne(p => p.Parent)
			.WithMany(p => p.Children)
			.HasForeignKey(p => p.ParentId);
	}
}
