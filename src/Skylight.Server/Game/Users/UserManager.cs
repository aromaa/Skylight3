using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Users;

internal sealed class UserManager : IUserManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public UserManager(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	public async ValueTask<IUserInfo?> GetUserInfoAsync(int id, CancellationToken cancellationToken)
	{
		return await this.LoadUserProfileAsync(id, cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<IUserProfile?> GetUserProfileAsync(int id, CancellationToken cancellationToken)
	{
		return await this.LoadUserProfileAsync(id, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IUserProfile?> LoadUserProfileAsync(int id, CancellationToken cancellationToken)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		UserEntity? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken).ConfigureAwait(false);
		if (user is null)
		{
			return null;
		}

		//TODO: Caching!

		return new UserInfo(user);
	}
}
