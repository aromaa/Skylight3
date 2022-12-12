using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Rooms.Sound;

namespace Skylight.Infrastructure.EntityConfigurations.Room.Sound;

internal sealed class SongEntityEntityTypeConfiguration : IEntityTypeConfiguration<SongEntity>
{
	public void Configure(EntityTypeBuilder<SongEntity> builder)
	{
		builder.ToTable("user_songs");

		builder.Property(s => s.Name)
			.HasMaxLength(32);
	}
}
