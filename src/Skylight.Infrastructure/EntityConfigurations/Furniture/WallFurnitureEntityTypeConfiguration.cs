using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal class WallFurnitureEntityTypeConfiguration : FurnitureEntityTypeConfiguration<WallFurnitureEntity>
{
	public override void Configure(EntityTypeBuilder<WallFurnitureEntity> builder)
	{
		builder.ToTable("furniture_wall");

		builder.HasKey(f => f.Id);

		base.Configure(builder);
	}
}
