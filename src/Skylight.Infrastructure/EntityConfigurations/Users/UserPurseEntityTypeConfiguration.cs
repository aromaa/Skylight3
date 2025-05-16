using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserPurseEntityTypeConfiguration
	: IEntityTypeConfiguration<UserPurseEntity>
{
	public void Configure(EntityTypeBuilder<UserPurseEntity> builder)
	{
		builder.ToTable(name: "user_purse", tableBuilder => tableBuilder
				.HasCheckConstraint(name: "ck_user_purse_balance_non_negative", sql: "\"balance\" >= 0"));

		builder.HasKey(e => new { e.UserId, e.Currency })
			.HasName("pk_user_purse");

		builder.HasOne(e => e.User)
			.WithMany(u => u.Purse)
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("fk_user_purse_user");

		builder.Property(e => e.Currency)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(e => e.Balance)
			.IsRequired()
			.HasColumnType("integer")
			.HasDefaultValue(0);
	}
}
