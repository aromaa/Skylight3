using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class PrincipalDefaultsRankEntityTypeConfiguration : IEntityTypeConfiguration<PrincipalDefaultsRankEntity>
{
	public void Configure(EntityTypeBuilder<PrincipalDefaultsRankEntity> builder)
	{
		builder.ToTable("principal_defaults_ranks");

		builder.HasKey(e => new { e.Identifier, e.RankId });
	}
}
