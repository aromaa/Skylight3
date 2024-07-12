using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Items;

namespace Skylight.Infrastructure.EntityConfigurations.Items;

internal sealed class FloorItemDataEntityTypeConfiguration : IEntityTypeConfiguration<FloorItemDataEntity>
{
	public void Configure(EntityTypeBuilder<FloorItemDataEntity> builder)
	{
		builder.ToTable("items_floor_data");

		builder.HasKey(e => e.FloorItemId);
	}
}
