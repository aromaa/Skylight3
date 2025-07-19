using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Catalog;

namespace Skylight.Infrastructure.EntityConfigurations.Catalog;

internal sealed class CatalogOfferEntityTypeConfiguration : IEntityTypeConfiguration<CatalogOfferEntity>
{
	public void Configure(EntityTypeBuilder<CatalogOfferEntity> builder)
	{
		builder.ToTable("catalog_offers");

		builder.HasKey(o => o.Id);

		builder.Property(o => o.Name)
			.HasMaxLength(64);

		builder.Property(o => o.OrderNum)
			.HasDefaultValue(0)
			.ValueGeneratedNever();

		builder.Property(o => o.RentTime)
			.HasDefaultValue(TimeSpan.Zero)
			.ValueGeneratedNever();

		builder.Property(o => o.HasOffer)
			.HasDefaultValue(false)
			.ValueGeneratedNever();

		builder.HasOne(o => o.Page)
			.WithMany(p => p.Offers)
			.HasForeignKey(o => o.PageId);
	}
}
