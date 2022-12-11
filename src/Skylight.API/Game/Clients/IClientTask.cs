namespace Skylight.API.Game.Clients;

public interface IClientTask
{
	public Task ExecuteAsync(IClient client);
}
