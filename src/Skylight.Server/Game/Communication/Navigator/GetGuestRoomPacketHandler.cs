using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetGuestRoomPacketHandler<T> : UserPacketHandler<T>
	where T : IGetGuestRoomIncomingPacket
{
	private readonly INavigatorManager navigatorManager;

	public GetGuestRoomPacketHandler(INavigatorManager navigatorManager)
	{
		this.navigatorManager = navigatorManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(new GetGuestRoomTask
		{
			NavigatorManager = this.navigatorManager,

			RoomId = packet.RoomId,

			EnterRoom = packet.EnterRoom,
			RoomForward = packet.RoomForward,
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct GetGuestRoomTask : IClientTask
	{
		internal INavigatorManager NavigatorManager { get; init; }

		internal int RoomId { get; init; }

		internal bool EnterRoom { get; init; }
		internal bool RoomForward { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			IRoomInfo? room = await this.NavigatorManager.GetRoomDataAsync(this.RoomId).ConfigureAwait(false);
			if (room is null)
			{
				return;
			}

			client.SendAsync(new GetGuestRoomResultOutgoingPacket(this.EnterRoom, this.RoomForward, new GuestRoomData(room.Id, room.Name, room.Owner.Username, room.Layout.Id, 0), false, false, false, false, (0, 0, 0), (0, 1, 1, 1, 50)));
		}
	}
}
