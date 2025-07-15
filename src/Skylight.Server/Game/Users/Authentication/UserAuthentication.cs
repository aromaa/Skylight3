using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Badges;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Inventory.Items;
using Skylight.API.Game.Permissions;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Redis;
using StackExchange.Redis;

namespace Skylight.Server.Game.Users.Authentication;

internal sealed class UserAuthentication(RedisConnector redis, IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager, IPermissionManager permissionManager, IRoomManager roomManager, IBadgeManager badgeManager, IFurnitureManager furnitureManager, IFurnitureInventoryItemStrategy furnitureInventoryItemFactory)
	: IUserAuthentication
{
	private static readonly RedisKey redisSsoTicketKeyPrefix = new("sso-ticket:");
	private static readonly RedisValue[] redisSsoTicketValues = ["user-id", "user-ip"];

	private readonly RedisConnector redis = redis;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IUserManager userManager = userManager;
	private readonly IPermissionManager permissionManager = permissionManager;
	private readonly IRoomManager roomManager = roomManager;

	private readonly LoadContext loadContext = new(badgeManager, furnitureManager, furnitureInventoryItemFactory);

	public async Task<int?> AuthenticateAsync(IClient client, string ssoTicket, CancellationToken cancellationToken)
	{
		RedisKey ssoKey = UserAuthentication.redisSsoTicketKeyPrefix.Append(ssoTicket);

		IDatabase redis = await this.redis.GetDatabaseAsync().ConfigureAwait(false);
		IBatch batch = redis.CreateBatch();
		Task<RedisValue[]> hashGetResult = batch.HashGetAsync(ssoKey, UserAuthentication.redisSsoTicketValues);
		_ = batch.KeyDeleteAsync(ssoKey, CommandFlags.FireAndForget);
		batch.Execute();

		if (await hashGetResult.ConfigureAwait(false) is not [RedisValue ssoUserId, RedisValue ssoIp] || ssoUserId.IsNull)
		{
			return null;
		}

		return (int)ssoUserId;
	}

	public async Task<int?> AuthenticateAsync(IClient client, string username, string password, CancellationToken cancellationToken = default)
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

		int userId = await dbContext.Users
			.Where(u => u.Username == username)
			.Select(u => u.Id)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

		return userId;
	}

	public async Task<IUser?> LoginAsync(IClient client, int userId, CancellationToken cancellationToken = default)
	{
		IUserProfile? profile = await this.userManager.LoadUserProfileAsync(userId, cancellationToken).ConfigureAwait(false);
		if (profile is null)
		{
			return null;
		}

		IPermissionDirectory<int> userPermissionDirectory = await this.permissionManager.GetUserDirectoryAsync(cancellationToken).ConfigureAwait(false);
		IPermissionSubject? permissionSubject = await userPermissionDirectory.GetSubjectAsync(userId).ConfigureAwait(false);
		if (permissionSubject is null)
		{
			return null;
		}

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		UserSettingsEntity? userSettings = await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == profile.Id, cancellationToken).ConfigureAwait(false);
		User user = new(this.roomManager, client, profile, permissionSubject, new UserSettings(userSettings));

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
