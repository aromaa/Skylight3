using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Figure;

namespace Skylight.Infrastructure.EntityConfigurations.Figure;

internal sealed class FigureValidationEntityTypeConfiguration : IEntityTypeConfiguration<FigureValidationEntity>
{
	public void Configure(EntityTypeBuilder<FigureValidationEntity> builder)
	{
		builder.ToTable("figure_validation");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Name)
			.HasMaxLength(64);

		builder.HasMany(e => e.SetTypeRules)
			.WithOne(e => e.Validation)
			.HasForeignKey(e => e.ValidationId);

		builder.HasIndex(e => new { e.Name, e.Sex });
	}
}
