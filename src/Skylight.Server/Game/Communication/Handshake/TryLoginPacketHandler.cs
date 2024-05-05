using System.Text;
using Microsoft.Extensions.Options;
using Net.Communication.Attributes;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Server.Net;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class TryLoginPacketHandler<T>(IUserAuthentication userAuthentication, IClientManager clientManager, Lazy<ILoadableServiceManager> loadableServiceManager, IOptions<NetworkSettings> networkSettings)
	: ClientPacketHandler<T>
	where T : ITryLoginIncomingPacket
{
	private readonly IUserAuthentication userAuthentication = userAuthentication;
	private readonly IClientManager clientManager = clientManager;

	private readonly Lazy<ILoadableServiceManager> loadableServiceManager = loadableServiceManager;

	private readonly NetworkSettings networkSettings = networkSettings.Value;

	internal override void Handle(IClient client, in T packet)
	{
		if (client.User is not null)
		{
			return;
		}

		string username = Encoding.UTF8.GetString(packet.Username);
		string password = Encoding.UTF8.GetString(packet.Password);

		client.ScheduleTask(async client =>
		{
			if (client.User is not null)
			{
				return;
			}

			if (!this.networkSettings.EarlyAccept)
			{
				await this.loadableServiceManager.Value.WaitForInitialization().ConfigureAwait(false);
			}

			IUser? user = await this.userAuthentication.LoginAsync(client, username, password).ConfigureAwait(false);
			if (user is null)
			{
				return;
			}

			if (!this.clientManager.TryAdd(client, user))
			{
				return;
			}

			client.Authenticate(user);
		});
	}
}
