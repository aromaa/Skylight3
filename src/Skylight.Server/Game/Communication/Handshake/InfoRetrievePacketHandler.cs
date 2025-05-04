using System.Text;
using Microsoft.Extensions.Options;
using Net.Communication.Attributes;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Server.Net;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class InfoRetrievePacketHandler<T>(IUserAuthentication userAuthentication, IClientManager clientManager, Lazy<ILoadableServiceManager> loadableServiceManager, IOptions<NetworkSettings> networkSettings) : ClientPacketHandler<T>
	where T : IInfoRetrieveIncomingPacket
{
	private readonly IUserAuthentication userAuthentication = userAuthentication;
	private readonly IClientManager clientManager = clientManager;

	private readonly Lazy<ILoadableServiceManager> loadableServiceManager = loadableServiceManager;

	private readonly NetworkSettings networkSettings = networkSettings.Value;

	internal override void Handle(IClient client, in T packet)
	{
		if (client.User is { } user)
		{
			this.SendUserObject(user);

			return;
		}

		string username = client.Encoding.GetString(packet.Username);
		string password = client.Encoding.GetString(packet.Password);
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
		{
			return;
		}

		client.ScheduleTask(async client =>
		{
			if (client.User is { } user)
			{
				this.SendUserObject(user);

				return;
			}

			if (!this.networkSettings.EarlyAccept)
			{
				await this.loadableServiceManager.Value.WaitForInitialization().ConfigureAwait(false);
			}

			int? userId = await this.userAuthentication.AuthenticateAsync(client, username, password).ConfigureAwait(false);
			if (userId is null)
			{
				return;
			}

			await this.clientManager.LoginAsync(client, userId.Value).ConfigureAwait(false);

			this.SendUserObject(client.User!);
		});
	}

	private void SendUserObject(IUser user)
	{
		user.SendAsync(new UserObjectOutgoingPacket
		{
			UserId = user.Profile.Id,
			Username = user.Profile.Username,
			Figure = user.Profile.Figure,
			Gender = user.Profile.Gender,
			CustomData = string.Empty,
			Tickets = 0,
			SwimSuit = string.Empty,
			Film = 0,
			RealName = string.Empty,
			DirectMail = false,
			RespectTotal = 0,
			RespectLeft = 0,
			PerRespectLeft = 0,
			StreamPublishingAllowed = false,
			LastAccessDate = user.Profile.LastOnline,
			NameChangeAllowed = true,
			AccountSafetyLocked = false
		});
	}
}
