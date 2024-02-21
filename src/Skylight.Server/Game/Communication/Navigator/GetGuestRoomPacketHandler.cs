using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class GetGuestRoomPacketHandler<T>(INavigatorManager navigatorManager) : UserPacketHandler<T>
	where T : IGetGuestRoomIncomingPacket
{
	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		int roomId = packet.RoomId;

		bool enterRoom = packet.EnterRoom;
		bool roomForward = packet.RoomForward;

		user.Client.ScheduleTask(async client =>
		{
			IRoomInfo? room = await this.navigatorManager.GetRoomDataAsync(roomId).ConfigureAwait(false);
			if (room is null)
			{
				return;
			}

			client.SendAsync(new GetGuestRoomResultOutgoingPacket(enterRoom, roomForward, new GuestRoomData(room.Id, room.Name, room.Owner.Username, room.Layout.Id, 0), false, false, false, false, (0, 0, 0), (0, 1, 1, 1, 50)));
		});
	}
}
