using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Recycler.FurniMatic;

namespace Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;

internal sealed class FurniMaticPrizeLevelEntityTypeConfiguration : IEntityTypeConfiguration<FurniMaticPrizeLevelEntity>
{
	public void Configure(EntityTypeBuilder<FurniMaticPrizeLevelEntity> builder)
	{
		builder.ToTable("furnimatic_levels");

		builder.HasKey(l => l.Level);
	}
}
