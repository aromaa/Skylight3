using System.Runtime.InteropServices;
using System.Text;
using Net.Communication.Attributes;
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

namespace Skylight.Server.Game.Communication.Handshake;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class SSOTicketPacketHandler<T> : ClientPacketHandler<T>
	where T : ISSOTicketIncomingPacket
{
	private readonly IUserAuthentication userAuthentication;
	private readonly IClientManager clientManager;

	public SSOTicketPacketHandler(IUserAuthentication userAuthentication, IClientManager clientManager)
	{
		this.userAuthentication = userAuthentication;
		this.clientManager = clientManager;
	}

	internal override void Handle(IClient client, in T packet)
	{
		if (client.User is not null)
		{
			return;
		}

		client.ScheduleTask(new SSOTicketTask
		{
			UserAuthentication = this.userAuthentication,
			ClientManager = this.clientManager,

			SSOTicket = Encoding.UTF8.GetString(packet.SSOTicket)
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct SSOTicketTask : IClientTask
	{
		internal IUserAuthentication UserAuthentication { get; init; }
		internal IClientManager ClientManager { get; init; }

		internal string SSOTicket { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			if (client.User is not null)
			{
				return;
			}

			IUser? user = await this.UserAuthentication.AuthenticateAsync(client, this.SSOTicket).ConfigureAwait(false);
			if (user is null)
			{
				return;
			}

			if (!this.ClientManager.TryAdd(client, user))
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
			client.SendAsync(new NavigatorSettingsOutgoingPacket(user.Settings.Home, user.Settings.Home));
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
		}
	}
}
