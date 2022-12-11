using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Navigator;

namespace Skylight.Infrastructure.EntityConfigurations.Navigator;

internal sealed class RoomFlatCatEntityTypeConfiguration : IEntityTypeConfiguration<RoomFlatCatEntity>
{
	public void Configure(EntityTypeBuilder<RoomFlatCatEntity> builder)
	{
		builder.ToTable("navigator_flatcats");

		builder.HasKey(f => f.Id);

		builder.Property(f => f.Caption)
			.HasMaxLength(64);
	}
}
