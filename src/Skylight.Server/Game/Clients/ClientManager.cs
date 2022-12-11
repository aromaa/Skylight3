using Net.Collections;
using Net.Sockets;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Clients;

internal sealed class ClientManager : IClientManager
{
	private readonly CriticalSocketCollection<IUser> clients;

	public ClientManager()
	{
		this.clients = new CriticalSocketCollection<IUser>(removeEvent: (ISocket socket, ref IUser data) =>
		{
			data.Client.Disconnect();
		});
	}

	public bool TryAdd(IClient client, IUser user)
	{
		return this.clients.TryAdd(client.Socket, user);
	}
}
