using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Domain.Items;
using Skylight.Infrastructure;
using StackExchange.Redis;

namespace Skylight.Server.Game.Users.Authentication;

internal sealed class UserAuthentication : IUserAuthentication
{
	private static readonly RedisKey redisSsoTicketKeyPrefix = new("sso-ticket:");
	private static readonly RedisValue[] redisSsoTicketValues = { "user-id", "user-ip" };

	private readonly IDatabase redis;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IUserManager userManager;
	private readonly IFurnitureManager furnitureManager;
	private readonly IFurnitureInventoryItemStrategy furnitureInventoryItemFactory;

	public UserAuthentication(IConnectionMultiplexer redis, IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager, IFurnitureManager furnitureManager, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	{
		this.redis = redis.GetDatabase();

		this.dbContextFactory = dbContextFactory;

		this.userManager = userManager;
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

		User user = new(client, profile);

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
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
		}

		return user;
	}
}
