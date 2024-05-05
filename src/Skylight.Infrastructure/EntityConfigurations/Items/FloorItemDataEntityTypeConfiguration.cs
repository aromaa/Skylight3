using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skylight.Infrastructure.EntityConfigurations.Items;

internal sealed class FloorItemDataEntityTypeConfiguration : IEntityTypeConfiguration<FloorItemDataEntity>
{
	public void Configure(EntityTypeBuilder<FloorItemDataEntity> builder)
	{
		builder.ToTable("items_floor_data");

		builder.HasKey(e => e.FloorItemId);
	}
}
