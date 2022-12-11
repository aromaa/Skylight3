using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skylight.Domain.Server;

namespace Skylight.Infrastructure.EntityConfigurations.Server;

internal sealed class SettingsEntityTypeConfiguration : IEntityTypeConfiguration<SettingsEntity>
{
	public void Configure(EntityTypeBuilder<SettingsEntity> builder)
	{
		builder.ToTable("settings");

		builder.HasKey(s => s.Id);

		builder.Property(s => s.Id)
			.HasMaxLength(255);

		builder.Property(s => s.Value)
			.HasMaxLength(255);
	}
}
