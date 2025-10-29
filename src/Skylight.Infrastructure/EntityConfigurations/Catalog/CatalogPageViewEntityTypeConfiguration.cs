using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal abstract class CatalogPageViewEntityTypeConfiguration<TCatalog, TView, TOffer> : IEntityTypeConfiguration<TView>
	where TCatalog : CatalogEntity<TCatalog, TView, TOffer>
	where TView : CatalogPageViewEntity<TCatalog, TView, TOffer>
	where TOffer : CatalogPageOfferEntity<TCatalog, TView, TOffer>
{
	public virtual void Configure(EntityTypeBuilder<TView> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(p => p.OrderNum)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(p => p.Visiblity)
			.HasDefaultValue(CatalogPageVisiblity.Enabled)
			.ValueGeneratedNever();

		builder.HasOne(e => e.Catalog)
			.WithMany(e => e.Views)
			.HasForeignKey(e => e.CatalogId);

		builder.HasOne(p => p.Parent)
			.WithMany(p => p.Children)
			.HasForeignKey(p => p.ParentId);

		builder.HasOne(e => e.Page)
			.WithMany()
			.HasForeignKey(e => e.PageId);

		builder.HasOne(e => e.AccessSet)
			.WithMany()
			.HasForeignKey(e => e.AccessSetId);

		builder.HasMany(e => e.Offers)
			.WithOne(e => e.View)
			.HasForeignKey(e => e.ViewId);
	}
}
