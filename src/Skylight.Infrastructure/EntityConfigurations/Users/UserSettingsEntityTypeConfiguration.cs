﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserSettingsEntityTypeConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
	public void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
	{
		builder.ToTable("user_settings");

		builder.HasKey(u => u.UserId);

		builder.HasOne(u => u.User)
			.WithOne(u => u.Settings)
			.HasForeignKey<UserSettingsEntity>(u => u.UserId);

		builder.HasOne(u => u.HomeRoom)
			.WithMany()
			.HasForeignKey(u => u.HomeRoomId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.Property(u => u.UiVolume)
			.HasDefaultValue(75);

		builder.Property(u => u.FurniVolume)
			.HasDefaultValue(75);

		builder.Property(u => u.TraxVolume)
			.HasDefaultValue(75);
	}
}
