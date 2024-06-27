using System.Text;
using Net.Sockets;
using Skylight.API.Game.Users;
using Skylight.API.Net;

namespace Skylight.API.Game.Clients;

public interface IClient : IPacketSender
{
	public ISocket Socket { get; }
	public Encoding Encoding { get; }

	public IUser? User { get; }

	public void Authenticate(IUser user);

	public bool ScheduleTask<T>(in T task)
		where T : IClientTask;

	public bool ScheduleTask(Func<IClient, Task> task);

	public void Disconnect();
}
