using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Items;

namespace Skylight.Infrastructure.EntityConfigurations.Items;

internal sealed class PublicRoomItemEntityTypeConfiguration : IEntityTypeConfiguration<PublicRoomItemEntity>
{
	public void Configure(EntityTypeBuilder<PublicRoomItemEntity> builder)
	{
		builder.ToTable("items_public");

		builder.HasKey(i => i.Id);

		builder.HasOne(i => i.Layout)
			.WithMany()
			.HasForeignKey(i => i.LayoutId);
	}
}
