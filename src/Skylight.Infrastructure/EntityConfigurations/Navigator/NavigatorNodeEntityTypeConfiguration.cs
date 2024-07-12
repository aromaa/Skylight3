using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Navigator;

namespace Skylight.Infrastructure.EntityConfigurations.Navigator;

internal sealed class NavigatorNodeEntityTypeConfiguration : IEntityTypeConfiguration<NavigatorNodeEntity>
{
	public void Configure(EntityTypeBuilder<NavigatorNodeEntity> builder)
	{
		builder.ToTable("navigator_nodes");

		builder.Property(f => f.Caption)
			.HasMaxLength(64);
	}
}
