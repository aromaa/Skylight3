using Microsoft.EntityFrameworkCore;
using Skylight.Domain.Achievements;
using Skylight.Domain.Badges;
using Skylight.Domain.Catalog;
using Skylight.Domain.Furniture;
using Skylight.Domain.Items;
using Skylight.Domain.Navigator;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Domain.Rooms;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Sound;
using Skylight.Domain.Server;
using Skylight.Domain.Users;
using Skylight.Infrastructure.EntityConfigurations.Achievements;
using Skylight.Infrastructure.EntityConfigurations.Badges;
using Skylight.Infrastructure.EntityConfigurations.Catalog;
using Skylight.Infrastructure.EntityConfigurations.Furniture;
using Skylight.Infrastructure.EntityConfigurations.Items;
using Skylight.Infrastructure.EntityConfigurations.Navigator;
using Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;
using Skylight.Infrastructure.EntityConfigurations.Room;
using Skylight.Infrastructure.EntityConfigurations.Room.Layout;
using Skylight.Infrastructure.EntityConfigurations.Room.Sound;
using Skylight.Infrastructure.EntityConfigurations.Server;
using Skylight.Infrastructure.EntityConfigurations.Users;

namespace Skylight.Infrastructure;

public sealed class SkylightContext(DbContextOptions<SkylightContext> options) : DbContext(options)
{
	public DbSet<AchievementEntity> Achievements { get; init; } = null!;
	public DbSet<AchievementLevelEntity> AchievementLevels { get; init; } = null!;

	public DbSet<BadgeEntity> Badges { get; init; } = null!;
	public DbSet<UserBadgeEntity> UserBadges { get; init; } = null!;

	public DbSet<CatalogBadgeProductEntity> CatalogBadgeProducts { get; init; } = null!;
	public DbSet<CatalogFloorProductEntity> CatalogFloorProducts { get; init; } = null!;
	public DbSet<CatalogOfferEntity> CatalogOffers { get; init; } = null!;
	public DbSet<CatalogPageEntity> CatalogPages { get; init; } = null!;
	public DbSet<CatalogProductEntity> CatalogProducts { get; init; } = null!;
	public DbSet<CatalogWallProductEntity> CatalogWallProducts { get; init; } = null!;

	public DbSet<FloorFurnitureEntity> FloorFurniture { get; init; } = null!;
	public DbSet<WallFurnitureEntity> WallFurniture { get; init; } = null!;

	public DbSet<FloorItemDataEntity> FloorItemsData { get; init; } = null!;
	public DbSet<FloorItemEntity> FloorItems { get; init; } = null!;
	public DbSet<PublicRoomItemEntity> PublicRoomItems { get; init; } = null!;
	public DbSet<WallItemDataEntity> WallItemsData { get; init; } = null!;
	public DbSet<WallItemEntity> WallItems { get; init; } = null!;

	public DbSet<RoomFlatCatEntity> FlatCats { get; init; } = null!;

	public DbSet<FurniMaticFloorItemEntity> FurniMaticFloorItems { get; init; } = null!;
	public DbSet<FurniMaticGiftEntity> FurniMaticGifts { get; init; } = null!;
	public DbSet<FurniMaticItemEntity> FurniMaticItems { get; init; } = null!;
	public DbSet<FurniMaticPrizeEntity> FurniMaticPrizes { get; init; } = null!;
	public DbSet<FurniMaticPrizeLevelEntity> FurniMaticPrizeLevels { get; init; } = null!;
	public DbSet<FurniMaticWallItemEntity> FurniMaticWallItems { get; init; } = null!;

	public DbSet<RoomLayoutEntity> RoomLayouts { get; init; } = null!;

	public DbSet<SongEntity> Songs { get; init; } = null!;

	public DbSet<RoomEntity> Rooms { get; init; } = null!;

	public DbSet<SettingsEntity> Settings { get; init; } = null!;

	public DbSet<UserEntity> Users { get; init; } = null!;

	public DbSet<UserSettingsEntity> UserSettings { get; init; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new AchievementEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new AchievementLevelEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new BadgeEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new UserBadgeEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new CatalogBadgeProductEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogFloorProductEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogOfferEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogPageEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogProductEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new CatalogWallProductEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FloorFurnitureEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new WallFurnitureEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FloorItemDataEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FloorItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new PublicRoomItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new WallItemDataEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new WallItemEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new RoomFlatCatEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FurniMaticFloorItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticGiftEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticPrizeEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticPrizeLevelEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticWallItemEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new RoomLayoutEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new SongEntityEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new SettingsEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new UserSettingsEntityTypeConfiguration());
	}
}
