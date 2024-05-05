using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skylight.Infrastructure.EntityConfigurations.Items;

internal sealed class WallItemDataEntityTypeConfiguration : IEntityTypeConfiguration<WallItemDataEntity>
{
	public void Configure(EntityTypeBuilder<WallItemDataEntity> builder)
	{
		builder.ToTable("items_wall_data");

		builder.HasKey(e => e.WallItemId);
	}
}
