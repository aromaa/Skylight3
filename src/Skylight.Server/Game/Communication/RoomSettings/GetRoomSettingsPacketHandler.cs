using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.RoomSettings;
using Skylight.Protocol.Packets.Incoming.RoomSettings;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.RoomSettings;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.RoomSettings;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetRoomSettingsPacketHandler<T>(INavigatorManager navigatorManager) : UserPacketHandler<T>
	where T : IGetRoomSettingsIncomingPacket
{
	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		int roomId = packet.RoomId;

		user.Client.ScheduleTask(async client =>
		{
			IPrivateRoomInfo? roomInfo = await this.navigatorManager.GetPrivateRoomInfoAsync(roomId).ConfigureAwait(false);
			if (roomInfo is null)
			{
				client.SendAsync(new NoSuchFlatOutgoingPacket(roomId));

				return;
			}

			IRoomSettings settings = roomInfo.Settings;
			IRoomCustomizationSettings customizationSettings = settings.CustomizationSettings;

			client.SendAsync(new RoomSettingsDataOutgoingPacket(roomInfo.Id, settings.Name, settings.Description, settings.Category.Value.Id, settings.Tags, settings.EntryMode.ToProtocol(),
				settings.UsersMax, 100, settings.TradeMode.ToProtocol(), settings.WalkThrough, settings.AllowPets, settings.AllowPetsToEat, true, customizationSettings.HideWalls, customizationSettings.FloorThickness,
				customizationSettings.WallThickness, new RoomChatSettingsData(0, 0, 0, 99, 0), new RoomModerationSettingsData(0, 0, 0)));
		});
	}
}
