using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class AccessSetEntityTypeConfiguration : IEntityTypeConfiguration<AccessSetEntity>
{
	public void Configure(EntityTypeBuilder<AccessSetEntity> builder)
	{
		builder.ToTable("access_sets");
	}
}
