using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;

namespace Skylight.Server.Game.Users;

internal sealed class UserManager : IUserManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly AsyncTypedCache<int, IUserInfo?> userInfos;
	private readonly AsyncTypedCache<int, IUserProfile?> userProfiles;

	public UserManager(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;

		this.userInfos = new AsyncTypedCache<int, IUserInfo?>(null!);
		this.userProfiles = new AsyncTypedCache<int, IUserProfile?>(this.InternalLoadUserProfileAsync);
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

		UserEntity? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id).ConfigureAwait(false);
		if (user is null)
		{
			return null;
		}

		return new UserInfo(user);
	}
}
