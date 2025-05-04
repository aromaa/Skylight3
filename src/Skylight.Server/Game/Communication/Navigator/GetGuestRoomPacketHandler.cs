using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.RoomSettings;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(IGamePacketManager))]
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
			IPrivateRoomInfo? room = await this.navigatorManager.GetPrivateRoomInfoAsync(roomId).ConfigureAwait(false);
			if (room is null)
			{
				return;
			}

			client.SendAsync(new GetGuestRoomResultOutgoingPacket(enterRoom, roomForward, room.BuildGuestRoomData(), false, false, false, false, new RoomModerationSettingsData(1, 1, 1), new RoomChatSettingsData(0, 0, 0, 0, 0)));
		});
	}
}
