using Microsoft.EntityFrameworkCore;
using Skylight.API.Collections.Cache;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;
using Skylight.Server.Scheduling;

namespace Skylight.Server.Game.Users;

internal sealed class UserManager : IUserManager
{
	private readonly DatabaseBackgroundWorker databaseBackgroundWorker;
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFigureConfigurationManager figureConfigurationManager;

	private readonly AsyncCache<int, IUserInfo> userInfos;
	private readonly AsyncCache<int, IUserProfile> userProfiles;

	public UserManager(DatabaseBackgroundWorker databaseBackgroundWorker, IDbContextFactory<SkylightContext> dbContextFactory, IFigureConfigurationManager figureConfigurationManager)
	{
		this.databaseBackgroundWorker = databaseBackgroundWorker;
		this.dbContextFactory = dbContextFactory;
		this.figureConfigurationManager = figureConfigurationManager;

		this.userInfos = new AsyncCache<int, IUserInfo>(this.InternalLoadUserInfoAsync);
		this.userProfiles = new AsyncCache<int, IUserProfile>(this.InternalLoadUserProfileAsync);
	}

	public ValueTask<IUserInfo?> GetUserInfoAsync(int id, CancellationToken cancellationToken = default)
	{
		return this.userInfos.GetAsync(id);
	}

	public ValueTask<IUserProfile?> GetUserProfileAsync(int id, CancellationToken cancellationToken = default)
	{
		return this.userProfiles.GetAsync(id);
	}

	public Task<IUserProfile?> LoadUserProfileAsync(int userId, CancellationToken cancellationToken = default) => this.InternalLoadUserProfileAsync(userId);

	private async Task<IUserInfo?> InternalLoadUserInfoAsync(int id)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

		UserEntity? user = await dbContext.Users
			.Include(e => e.FigureSets!)
			.ThenInclude(e => e.Colors)
			.AsNoTracking()
			.AsSplitQuery()
			.FirstOrDefaultAsync(u => u.Id == id)
			.ConfigureAwait(false);
		if (user is null)
		{
			return null;
		}

		return new UserInfo(this.databaseBackgroundWorker, user, await this.figureConfigurationManager.GetAsync().ConfigureAwait(false));
	}

	private async Task<IUserProfile?> InternalLoadUserProfileAsync(int id)
	{
		ICacheReference<IUserInfo>? userInfo = await this.userInfos.GetValueAsync(id).ConfigureAwait(false);
		if (userInfo is null)
		{
			return null;
		}

		return new UserProfile(userInfo);
	}
}
