using System.Text;
using Microsoft.Extensions.Options;
using Net.Communication.Attributes;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Authentication;
using Skylight.Protocol.Packets.Data.CallForHelp;
using Skylight.Protocol.Packets.Data.Perk;
using Skylight.Protocol.Packets.Incoming.Handshake;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Availability;
using Skylight.Protocol.Packets.Outgoing.CallForHelp;
using Skylight.Protocol.Packets.Outgoing.Handshake;
using Skylight.Protocol.Packets.Outgoing.Navigator;
using Skylight.Protocol.Packets.Outgoing.Notifications;
using Skylight.Protocol.Packets.Outgoing.Perk;
using Skylight.Server.Net;

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class SSOTicketPacketHandler<T>(IUserAuthentication userAuthentication, IClientManager clientManager, Lazy<ILoadableServiceManager> loadableServiceManager, IOptions<NetworkSettings> networkSettings)
	: ClientPacketHandler<T>
	where T : ISSOTicketIncomingPacket
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

		string ssoTicket = Encoding.UTF8.GetString(packet.SSOTicket);

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

			IUser? user = await this.userAuthentication.AuthenticateAsync(client, ssoTicket).ConfigureAwait(false);
			if (user is null)
			{
				return;
			}

			if (!this.clientManager.TryAdd(client, user))
			{
				return;
			}

			client.Authenticate(user);

			//SSO stuff
			client.SendAsync(new AuthenticationOKOutgoingPacket());
			//Avatar effects
			//Unseen items
			//Figure set ids
			client.SendAsync(new NoobnessLevelOutgoingPacket(0));
			client.SendAsync(new NavigatorSettingsOutgoingPacket(user.Settings.HomeRoomId, user.Settings.HomeRoomId));
			client.SendAsync(new UserRightsOutgoingPacket(7, 7, true));
			//Favourites
			client.SendAsync(new AvailabilityStatusOutgoingPacket(true, false, true));
			client.SendAsync(new InfoFeedEnableOutgoingPacket(true));
			//Activity points
			//Achievement score
			client.SendAsync(new IsFirstLoginOfDayOutgoingPacket(true));
			//Mystery box keys
			//Builder club sub
			client.SendAsync(new CfhTopicsInitOutgoingPacket(Array.Empty<CallForHelpCategoryData>()));

			client.SendAsync(new PerkAllowancesOutgoingPacket
			{
				Perks = new List<PerkAllowanceData>
				{
					new("USE_GUIDE_TOOL", string.Empty, true),
					new("GIVE_GUIDE_TOURS", string.Empty, true),
					new("JUDGE_CHAT_REVIEWS", string.Empty, true),
					new("VOTE_IN_COMPETITIONS", string.Empty, true),
					new("SAFE_CHAT", string.Empty, true),
					new("FULL_CHAT", string.Empty, true),
					new("CALL_ON_HELPERS", string.Empty, true),
					new("CITIZEN", string.Empty, true),
					new("TRADE", string.Empty, true),
					new("BUILDER_AT_WORK", string.Empty, true),
					new("CAMERA", string.Empty, true),
					new("NAVIGATOR_ROOM_THUMBNAIL_CAMERA", string.Empty, true),
					new("MOUSE_ZOOM", string.Empty, true),
					new("NAVIGATOR_PHASE_ONE_2014", string.Empty, true),
					new("NAVIGATOR_PHASE_TWO_2014", string.Empty, true)
				}
			});
		});
	}
}
