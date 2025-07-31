using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;

namespace Skylight.Server.Game.Users;

internal sealed class UserManager : IUserManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IFigureConfigurationManager figureConfigurationManager;

	private readonly AsyncCache<int, IUserInfo> userInfos;
	private readonly AsyncCache<int, IUserProfile> userProfiles;

	public UserManager(IDbContextFactory<SkylightContext> dbContextFactory, IFigureConfigurationManager figureConfigurationManager)
	{
		this.dbContextFactory = dbContextFactory;
		this.figureConfigurationManager = figureConfigurationManager;

		this.userInfos = new AsyncCache<int, IUserInfo>(null!);
		this.userProfiles = new AsyncCache<int, IUserProfile>(this.InternalLoadUserProfileAsync);
	}

	public async ValueTask<IUserInfo?> GetUserInfoAsync(int id, CancellationToken cancellationToken = default)
	{
		return await this.userProfiles.GetAsync(id).ConfigureAwait(false);
	}

	public ValueTask<IUserProfile?> GetUserProfileAsync(int id, CancellationToken cancellationToken = default)
	{
		return this.userProfiles.GetAsync(id);
	}

	public Task<IUserProfile?> LoadUserProfileAsync(int userId, CancellationToken cancellationToken = default) => this.InternalLoadUserProfileAsync(userId);

	public async Task<IUserProfile?> InternalLoadUserProfileAsync(int id)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

		UserEntity? user = await dbContext.Users
			.Include(e => e.FigureSets!)
			.ThenInclude(e => e.Colors)
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == id)
			.ConfigureAwait(false);
		if (user is null)
		{
			return null;
		}

		return new UserInfo(user, await this.figureConfigurationManager.GetAsync().ConfigureAwait(false));
	}
}
