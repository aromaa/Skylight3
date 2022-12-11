using Skylight.API.Game.Clients;

namespace Skylight.API.Game.Users.Authentication;

public interface IUserAuthentication
{
	public Task<IUser?> AuthenticateAsync(IClient client, string ssoTicket, CancellationToken cancellationToken = default);
}
