using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal sealed class FloorFurnitureEntityTypeConfiguration : IEntityTypeConfiguration<FloorFurnitureEntity>
{
	public void Configure(EntityTypeBuilder<FloorFurnitureEntity> builder)
	{
		builder.ToTable("furniture_floor");

		builder.Property(f => f.ClassName)
			.HasMaxLength(128);

		builder.HasKey(f => f.Id);

		builder.Property(f => f.Width)
			.HasDefaultValue(1);

		builder.Property(f => f.Length)
			.HasDefaultValue(1);

		builder.Property(f => f.Height)
			.HasDefaultValue(new List<double> { 0.01 });

		builder.Property(f => f.InteractionType)
			.HasDefaultValue("default");

		builder.Property(f => f.InteractionData)
			.HasDefaultValue("1");
	}
}
