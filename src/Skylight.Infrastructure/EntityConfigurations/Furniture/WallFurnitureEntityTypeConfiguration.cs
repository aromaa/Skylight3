using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal class WallFurnitureEntityTypeConfiguration : IEntityTypeConfiguration<WallFurnitureEntity>
{
	public void Configure(EntityTypeBuilder<WallFurnitureEntity> builder)
	{
		builder.ToTable("furniture_wall");

		builder.HasKey(f => f.Id);

		builder.Property(f => f.InteractionType)
			.HasDefaultValue("default");

		builder.Property(f => f.InteractionData)
			.HasDefaultValueSql("'1'::jsonb");
	}
}
