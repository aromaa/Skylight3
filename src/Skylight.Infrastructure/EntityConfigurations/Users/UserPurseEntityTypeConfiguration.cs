using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Users;
using Skylight.Infrastructure.Extensions;

namespace Skylight.Infrastructure.EntityConfigurations.Users;

internal sealed class UserPurseEntityTypeConfiguration : IEntityTypeConfiguration<UserPurseEntity>
{
	public void Configure(EntityTypeBuilder<UserPurseEntity> builder)
	{
		builder.ToTable("user_purse");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.CurrencyType)
			.HasMaxLength(100)
			.AddCheckConstraint(c => $"{c} LIKE '%_:_%'"); // Ambiguous without a valid key

		builder.Property(e => e.CurrencyData)
			.HasColumnType("jsonb");

		builder.Property(e => e.Balance)
			.HasColumnType("integer")
			.HasDefaultValue(0);

		builder.HasIndex(e => new { e.UserId, e.CurrencyType, e.CurrencyData })
			.IsUnique()
			.AreNullsDistinct();

		builder.HasOne(e => e.User)
			.WithMany(u => u.Purse)
			.HasForeignKey(e => e.UserId);

		// EF Core does not support exclude constraints.
		// EXCLUDE USING gist(currency_type WITH =, (currency_data IS NULL) WITH <>)
	}
}
