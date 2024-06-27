namespace Skylight.API.Game.Clients;

public interface IClientManager
{
	public Task<bool> LoginAsync(IClient client, int userId);
}
