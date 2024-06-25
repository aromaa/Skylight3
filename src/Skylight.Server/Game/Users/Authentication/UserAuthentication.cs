using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using StackExchange.Redis;

namespace Skylight.Server.Game.Users.Authentication;

internal sealed class UserAuthentication(IConnectionMultiplexer redis, IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager, IBadgeManager badgeManager, IFurnitureManager furnitureManager, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	: IUserAuthentication
{
	private static readonly RedisKey redisSsoTicketKeyPrefix = new("sso-ticket:");
	private static readonly RedisValue[] redisSsoTicketValues = ["user-id", "user-ip"];

	private readonly IDatabase redis = redis.GetDatabase();

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IUserManager userManager = userManager;

	private readonly LoadContext loadContext = new(badgeManager, furnitureManager, furnitureInventoryItemFactory);

	public async Task<IUser?> AuthenticateAsync(IClient client, string ssoTicket, CancellationToken cancellationToken)
	{
		RedisKey ssoKey = UserAuthentication.redisSsoTicketKeyPrefix.Append(ssoTicket);

		IBatch batch = this.redis.CreateBatch();
		Task<RedisValue[]> hashGetResult = batch.HashGetAsync(ssoKey, UserAuthentication.redisSsoTicketValues);
		_ = batch.KeyDeleteAsync(ssoKey, CommandFlags.FireAndForget);
		batch.Execute();

		if (await hashGetResult.ConfigureAwait(false) is not [RedisValue ssoUserId, RedisValue ssoIp] || ssoUserId.IsNull)
		{
			return null;
		}

		return await this.LoadAsync(client, (int)ssoUserId, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IUser?> LoginAsync(IClient client, string username, string password, CancellationToken cancellationToken = default)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		await dbContext.Users.Upsert(new UserEntity
		{
			Username = username
		}).On(u => u.Username)
		.WhenMatched(u => new UserEntity
		{
			LastOnline = DateTime.UtcNow
		}).RunAsync(cancellationToken).ConfigureAwait(false);

		UserEntity user = await dbContext.Users.FirstAsync(u => u.Username == username, cancellationToken).ConfigureAwait(false);

		return await this.LoadAsync(client, user.Id, cancellationToken).ConfigureAwait(false);
	}

	private async Task<User?> LoadAsync(IClient client, int userId, CancellationToken cancellationToken = default)
	{
		IUserProfile? profile = await this.userManager.LoadUserProfileAsync(userId, cancellationToken).ConfigureAwait(false);
		if (profile is null)
		{
			return null;
		}

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		UserSettingsEntity? userSettings = await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == profile.Id, cancellationToken).ConfigureAwait(false);
		User user = new(client, profile, new UserSettings(userSettings));

		await user.LoadAsync(dbContext, this.loadContext, cancellationToken).ConfigureAwait(false);

		return user;
	}

	internal sealed class LoadContext(IBadgeManager badgeManager, IFurnitureManager furnitureManager, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	{
		internal IBadgeManager BadgeManager { get; } = badgeManager;

		internal IFurnitureManager FurnitureManager { get; } = furnitureManager;
		internal IFurnitureInventoryItemStrategy FurnitureInventoryItemFactory { get; } = furnitureInventoryItemFactory;
	}
}
