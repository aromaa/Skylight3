using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal sealed class FloorFurnitureEntityTypeConfiguration : FurnitureEntityTypeConfiguration<FloorFurnitureEntity>
{
	public override void Configure(EntityTypeBuilder<FloorFurnitureEntity> builder)
	{
		builder.ToTable("furniture_floor");

		builder.HasKey(f => f.Id);

		builder.Property(f => f.Kind)
			.HasDefaultValue(128)
			.HasDefaultValue("walkable");

		builder.Property(f => f.Width)
			.HasDefaultValue(1);

		builder.Property(f => f.Length)
			.HasDefaultValue(1);

		builder.Property(f => f.Height)
			.HasDefaultValue(new List<double> { 0.01 });

		base.Configure(builder);
	}
}
