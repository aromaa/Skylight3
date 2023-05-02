using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserSettingsEntityTypeConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
	public void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
	{
		builder.ToTable("user_settings");

		builder.HasKey(u => u.UserId);

		builder.Property(u => u.HomeRoom)
			.HasDefaultValue(0);

		builder.HasOne(u => u.User)
			.WithOne(u => u.Settings)
			.HasForeignKey<UserSettingsEntity>(u => u.UserId);

		builder.HasOne(u => u.Room)
			.WithMany()
			.HasForeignKey(u => u.HomeRoom)
			.OnDelete(DeleteBehavior.SetNull);
	}
}
