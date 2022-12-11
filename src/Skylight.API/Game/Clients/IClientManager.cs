using Skylight.API.Game.Users;

namespace Skylight.API.Game.Clients;

public interface IClientManager
{
	public bool TryAdd(IClient client, IUser user);
}
