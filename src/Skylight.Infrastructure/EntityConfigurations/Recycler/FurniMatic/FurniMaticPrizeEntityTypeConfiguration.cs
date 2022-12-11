using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticPrizeEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticPrizeEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticPrizeEntity> builder)
	{
		builder.ToTable("furnimatic_prizes");

		builder.HasIndex(p => p.Id);

		builder.HasOne(p => p.PrizeLevel)
			.WithMany(l => l.Prizes)
			.HasForeignKey(p => p.Level);
	}
}
