using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
{
	public void Configure(EntityTypeBuilder<UserEntity> builder)
	{
		builder.ToTable("users");

		builder.HasKey(u => u.Id);

		builder.Property(u => u.Username)
			.HasMaxLength(16);

		builder.Property(u => u.Figure)
			.HasMaxLength(128)
			.HasDefaultValue(string.Empty);

		builder.Property(u => u.Gender)
			.HasMaxLength(1)
			.HasDefaultValue("M");

		builder.Property(u => u.Motto)
			.HasMaxLength(38)
			.HasDefaultValue(string.Empty);

		builder.Property(u => u.LastOnline)
			.HasDefaultValueSql("NOW()");

		builder.HasIndex(u => u.Username)
			.IsUnique();
	}
}
