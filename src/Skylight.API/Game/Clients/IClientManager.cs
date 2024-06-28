namespace Skylight.API.Game.Clients;

public interface IClientManager
{
	public bool TryAccept(IClient client);

	public Task<bool> LoginAsync(IClient client, int userId);
}
