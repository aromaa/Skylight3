using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class RankChildEntityTypeConfiguration : IEntityTypeConfiguration<RankChildEntity>
{
	public void Configure(EntityTypeBuilder<RankChildEntity> builder)
	{
		builder.ToTable("rank_children");

		builder.HasKey(e => new { e.RankId, e.ChildRankId });
	}
}
