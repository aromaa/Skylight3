﻿using Microsoft.EntityFrameworkCore;
using Npgsql;
using Skylight.Domain.Achievements;
using Skylight.Domain.Badges;
using Skylight.Domain.Catalog;
using Skylight.Domain.Furniture;
using Skylight.Domain.Items;
using Skylight.Domain.Navigator;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Private;
using Skylight.Domain.Rooms.Public;
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
using Skylight.Infrastructure.EntityConfigurations.Room.Layout;
using Skylight.Infrastructure.EntityConfigurations.Room.Private;
using Skylight.Infrastructure.EntityConfigurations.Room.Public;
using Skylight.Infrastructure.EntityConfigurations.Room.Sound;
using Skylight.Infrastructure.EntityConfigurations.Server;
using Skylight.Infrastructure.EntityConfigurations.Users;

namespace Skylight.Infrastructure;

public abstract class BaseSkylightContext(DbContextOptions options) : DbContext(options)
{
	public DbSet<AchievementEntity> Achievements { get; init; } = null!;
	public DbSet<AchievementLevelEntity> AchievementLevels { get; init; } = null!;

	public DbSet<BadgeEntity> Badges { get; init; } = null!;
	public DbSet<UserBadgeEntity> UserBadges { get; init; } = null!;
	public DbSet<UserWardrobeSlotEntity> UserWardrobe { get; init; } = null!;

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

	public DbSet<NavigatorCategoryNodeEntity> NavigatorCategoryNodes { get; init; } = null!;
	public DbSet<NavigatorNodeEntity> NavigatorNodes { get; init; } = null!;
	public DbSet<NavigatorPublicRoomNodeEntity> NavigatorPublicRooms { get; init; } = null!;

	public DbSet<FurniMaticFloorItemEntity> FurniMaticFloorItems { get; init; } = null!;
	public DbSet<FurniMaticGiftEntity> FurniMaticGifts { get; init; } = null!;
	public DbSet<FurniMaticItemEntity> FurniMaticItems { get; init; } = null!;
	public DbSet<FurniMaticPrizeEntity> FurniMaticPrizes { get; init; } = null!;
	public DbSet<FurniMaticPrizeLevelEntity> FurniMaticPrizeLevels { get; init; } = null!;
	public DbSet<FurniMaticWallItemEntity> FurniMaticWallItems { get; init; } = null!;

	public DbSet<CustomRoomLayoutEntity> CustomRoomLayouts { get; init; } = null!;
	public DbSet<RoomLayoutEntity> RoomLayouts { get; init; } = null!;

	public DbSet<PrivateRoomActivityEntity> PrivateRoomActivity { get; init; } = null!;
	public DbSet<PrivateRoomEntity> PrivateRooms { get; init; } = null!;

	public DbSet<PublicRoomEntity> PublicRooms { get; init; } = null!;
	public DbSet<PublicRoomWorldEntity> PublicRoomWorlds { get; init; } = null!;

	public DbSet<SongEntity> Songs { get; init; } = null!;

	public DbSet<SettingsEntity> Settings { get; init; } = null!;

	public DbSet<UserEntity> Users { get; init; } = null!;

	public DbSet<UserCurrenciesEntity> UserCurrencies { get; init; } = null!;
	public DbSet<UserSettingsEntity> UserSettings { get; init; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresEnum<PrivateRoomEntryMode>();
		modelBuilder.HasPostgresEnum<PrivateRoomTradeMode>();

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

		modelBuilder.ApplyConfiguration(new NavigatorCategoryNodeEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new NavigatorNodeEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new NavigatorPublicRoomNodeEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new FurniMaticFloorItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticGiftEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticItemEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticPrizeEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticPrizeLevelEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new FurniMaticWallItemEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new CustomRoomLayoutEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new RoomLayoutEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new PrivateRoomActivityEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new PrivateRoomEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new PublicRoomEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new PublicRoomWorldEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new SongEntityEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new SettingsEntityTypeConfiguration());

		modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new UserCurrenciesEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new UserSettingsEntityTypeConfiguration());
		modelBuilder.ApplyConfiguration(new UserWardrobeEntityTypeConfiguration());
	}

	public static NpgsqlDataSource CreateNpgsqlDataSource(string connectionString)
	{
		NpgsqlDataSourceBuilder builder = new(connectionString);
		builder.MapEnum<PrivateRoomEntryMode>();
		builder.MapEnum<PrivateRoomTradeMode>();

		return builder.Build();
	}

	public static DbContextOptionsBuilder ConfigureNpgsqlDbContextOptions(DbContextOptionsBuilder builder, string? connectionString)
	{
		builder.UseNpgsql(connectionString);
		builder.UseSnakeCaseNamingConvention();

		return builder;
	}
}
