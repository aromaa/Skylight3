using Skylight.API.Collections.Cache;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Users;

internal sealed class UserProfile : IUserProfile
{
	private readonly ICacheReference<IUserInfo> userInfo;

	internal UserProfile(ICacheReference<IUserInfo> userInfo)
	{
		this.userInfo = userInfo;
	}

	public IUserInfo Info => this.userInfo.Value;
}
