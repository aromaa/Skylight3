using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Furniture;

namespace Skylight.Infrastructure.EntityConfigurations.Furniture;

internal abstract class FurnitureEntityTypeConfiguration<T> : IEntityTypeConfiguration<T>
	where T : FurnitureEntity
{
	public virtual void Configure(EntityTypeBuilder<T> builder)
	{
		builder.Property(f => f.Revision)
			.HasMaxLength(16);

		builder.Property(f => f.ClassName)
			.HasMaxLength(128);

		builder.Property(f => f.InteractionType)
			.HasMaxLength(128)
			.HasDefaultValue("default");

		builder.Property(f => f.InteractionData)
			.HasMaxLength(1024 * 128)
			.HasDefaultValueSql("'1'::jsonb");
	}
}
