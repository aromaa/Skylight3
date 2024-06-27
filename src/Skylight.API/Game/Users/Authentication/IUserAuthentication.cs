using Skylight.API.Game.Clients;

namespace Skylight.API.Game.Users.Authentication;

public interface IUserAuthentication
{
	public Task<int?> AuthenticateAsync(IClient client, string ssoTicket, CancellationToken cancellationToken = default);
	public Task<int?> AuthenticateAsync(IClient client, string username, string password, CancellationToken cancellationToken = default);

	public Task<IUser?> LoginAsync(IClient client, int userId, CancellationToken cancellationToken = default);
}
