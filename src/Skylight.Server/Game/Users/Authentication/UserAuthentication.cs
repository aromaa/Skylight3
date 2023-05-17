using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Domain.Badges;
using Skylight.Domain.Items;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Game.Inventory.Items.Badges;
using StackExchange.Redis;

namespace Skylight.Server.Game.Users.Authentication;

internal sealed class UserAuthentication : IUserAuthentication
{
	private static readonly RedisKey redisSsoTicketKeyPrefix = new("sso-ticket:");
	private static readonly RedisValue[] redisSsoTicketValues = { "user-id", "user-ip" };

	private readonly IDatabase redis;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IUserManager userManager;

	private readonly IBadgeManager badgeManager;

	private readonly IFurnitureManager furnitureManager;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemFactory;

	public UserAuthentication(IConnectionMultiplexer redis, IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager, IBadgeManager badgeManager, IFurnitureManager furnitureManager, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	{
		this.redis = redis.GetDatabase();

		this.dbContextFactory = dbContextFactory;

		this.userManager = userManager;

		this.badgeManager = badgeManager;

		this.furnitureManager = furnitureManager;
		this.furnitureInventoryItemFactory = furnitureInventoryItemFactory;
	}

	public async Task<IUser?> AuthenticateAsync(IClient client, string ssoTicket, CancellationToken cancellationToken)
	{
		RedisValue[]? result = (RedisValue[]?)await this.redis.ScriptEvaluateAsync("""
		local result = redis.call('HMGET', KEYS[1], unpack(ARGV))
		redis.call('DEL', KEYS[1])
		return result
		""", new[]
		{
			UserAuthentication.redisSsoTicketKeyPrefix.Append(ssoTicket)
		}, UserAuthentication.redisSsoTicketValues).ConfigureAwait(false);

		if (result is not [RedisValue ssoUserId, RedisValue ssoIp] || ssoUserId.IsNull)
		{
			return null;
		}

		IUserProfile? profile = await this.userManager.LoadUserProfileAsync((int)ssoUserId, cancellationToken).ConfigureAwait(false);
		if (profile is null)
		{
			return null;
		}

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			UserSettingsEntity? userSettings = await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == profile.Id).ConfigureAwait(false);
			User user = new(client, profile, new UserSettings(userSettings));

			IBadgeSnapshot badges = this.badgeManager.Current;
			IFurnitureSnapshot furnitures = this.furnitureManager.Current;

			await foreach (FloorItemEntity item in dbContext.FloorItems
						 .AsNoTracking()
						 .Where(i => i.UserId == profile.Id && i.RoomId == null)
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken))
			{
				if (!furnitures.TryGetFloorFurniture(item.FurnitureId, out IFloorFurniture? furniture))
				{
					continue;
				}

				user.Inventory.TryAddFloorItem(this.furnitureInventoryItemFactory.CreateFurnitureItem(item.Id, profile, furniture, item.ExtraData));
			}

			await foreach (WallItemEntity item in dbContext.WallItems
						 .AsNoTracking()
						 .Where(i => i.UserId == profile.Id && i.RoomId == null)
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken))
			{
				if (!furnitures.TryGetWallFurniture(item.FurnitureId, out IWallFurniture? furniture))
				{
					continue;
				}

				user.Inventory.TryAddWallItem(this.furnitureInventoryItemFactory.CreateFurnitureItem(item.Id, profile, furniture, item.ExtraData));
			}

			await foreach (UserBadgeEntity entity in dbContext.UserBadges
							   .AsNoTracking()
							   .Where(b => b.UserId == profile.Id)
							   .AsAsyncEnumerable()
							   .WithCancellation(cancellationToken))
			{
				if (!badges.TryGetBadge(entity.BadgeCode, out IBadge? badge))
				{
					continue;
				}

				user.Inventory.TryAddBadge(new BadgeInventoryItem(badge, profile));
			}

			return user;
		}
	}
}
