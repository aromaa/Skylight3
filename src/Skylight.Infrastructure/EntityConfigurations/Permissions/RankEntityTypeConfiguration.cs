using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Permissions;

namespace Skylight.Infrastructure.EntityConfigurations.Permissions;

internal sealed class RankEntityTypeConfiguration : IEntityTypeConfiguration<RankEntity>
{
	public void Configure(EntityTypeBuilder<RankEntity> builder)
	{
		builder.ToTable("ranks");

		builder.HasKey(e => e.Id);

		builder.HasMany(e => e.Permissions)
			.WithOne(e => e.Rank)
			.HasForeignKey(e => e.RankId);

		builder.HasMany(e => e.Entitlements)
			.WithOne(e => e.Rank)
			.HasForeignKey(e => e.RankId);

		builder.HasMany(e => e.Children)
			.WithOne(e => e.ChildRank)
			.HasForeignKey(e => e.ChildRankId);
	}
}
