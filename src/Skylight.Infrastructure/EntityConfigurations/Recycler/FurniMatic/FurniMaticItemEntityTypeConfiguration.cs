using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticItemEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticItemEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticItemEntity> builder)
	{
		builder.UseTpcMappingStrategy();

		builder.HasKey(i => i.Id);

		builder.HasOne(i => i.Prize)
			.WithMany(p => p.Items)
			.HasForeignKey(i => i.PrizeId);
	}
}
