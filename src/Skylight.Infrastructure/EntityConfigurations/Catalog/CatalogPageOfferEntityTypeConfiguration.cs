using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal abstract class CatalogPageOfferEntityTypeConfiguration<TCatalog, TView, TOffer> : IEntityTypeConfiguration<TOffer>
	where TCatalog : CatalogEntity<TCatalog, TView, TOffer>
	where TView : CatalogPageViewEntity<TCatalog, TView, TOffer>
	where TOffer : CatalogPageOfferEntity<TCatalog, TView, TOffer>
{
	public virtual void Configure(EntityTypeBuilder<TOffer> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(o => o.OfferOrderNum)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(o => o.PageOrderNum)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.HasOne(e => e.View)
			.WithMany(e => e.Offers)
			.HasForeignKey(e => e.ViewId);

		builder.HasOne(e => e.Offer)
			.WithMany()
			.HasForeignKey(e => e.OfferId);

		builder.HasOne(e => e.Rank)
			.WithMany()
			.HasForeignKey(e => e.RankId);
	}
}
