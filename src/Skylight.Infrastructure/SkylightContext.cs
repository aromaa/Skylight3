using Microsoft.EntityFrameworkCore;
using Skylight.Domain.Catalog;
using Skylight.Domain.Furniture;
using Skylight.Domain.Items;
using Skylight.Domain.Navigator;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Domain.Rooms;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Server;
using Skylight.Domain.Users;
using Skylight.Infrastructure.EntityConfigurations.Catalog;
using Skylight.Infrastructure.EntityConfigurations.Furniture;
using Skylight.Infrastructure.EntityConfigurations.Items;
using Skylight.Infrastructure.EntityConfigurations.Navigator;
using Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;
using Skylight.Infrastructure.EntityConfigurations.Room;
using Skylight.Infrastructure.EntityConfigurations.Room.Layout;
using Skylight.Infrastructure.EntityConfigurations.Server;
using Skylight.Infrastructure.EntityConfigurations.Users;

namespace Skylight.Infrastructure;

public sealed class SkylightContext : DbContext
{
	public DbSet<CatalogOfferEntity> CatalogOffers { get; init; } = null!;
	public DbSet<CatalogPageEntity> CatalogPages { get; init; } = null!;
	public DbSet<CatalogProductEntity> CatalogProducts { get; init; } = null!;

	public DbSet<FloorFurnitureEntity> FloorFurniture { get; init; } = null!;
	public DbSet<WallFurnitureEntity> WallFurniture { get; init; } = null!;

	public DbSet<FloorItemEntity> FloorItems { get; init; } = null!;
	public DbSet<PublicRoomItemEntity> PublicRoomItems { get; init; } = null!;
	public DbSet<WallItemEntity> WallItems { get; init; } = null!;

	public DbSet<RoomFlatCatEntity> FlatCats { get; init; } = null!;

	public DbSet<FurniMaticGiftEntity> FurniMaticGifts { get; init; } = null!;
	public DbSet<FurniMaticItemEntity> FurniMaticItems { get; init; } = null!;
	public DbSet<FurniMaticPrizeEntity> FurniMaticPrizes { get; init; } = null!;
	public DbSet<FurniMaticPrizeLevelEntity> FurniMaticPrizeLevels { get; init; } = null!;

	public DbSet<RoomLayoutEntity> RoomLayouts { get; init; } = null!;

	public DbSet<RoomEntity> Rooms { get; init; } = null!;

	public DbSet<SettingsEntity> Settings { get; init; } = null!;

	public DbSet<UserEntity> Users { get; init; } = null!;

	public SkylightContext(DbContextOptions<SkylightContext> options)
		: base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSnakeCaseNamingConvention();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new CatalogOfferEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogPageEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogProductEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FloorFurnitureEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new WallFurnitureEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FloorItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new PublicRoomItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new WallItemEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new RoomFlatCatEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FurniMaticGiftEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticPrizeEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticPrizeLevelEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new RoomLayoutEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new SettingsEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
	}
}
