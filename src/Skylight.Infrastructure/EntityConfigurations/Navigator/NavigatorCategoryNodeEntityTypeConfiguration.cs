using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Navigator;

namespace Skylight.Infrastructure.EntityConfigurations.Navigator;

internal class NavigatorCategoryNodeEntityTypeConfiguration : IEntityTypeConfiguration<NavigatorCategoryNodeEntity>
{
	public void Configure(EntityTypeBuilder<NavigatorCategoryNodeEntity> builder)
	{
	}
}
