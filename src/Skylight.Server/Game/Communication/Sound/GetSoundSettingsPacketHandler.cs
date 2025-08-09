using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Preferences;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetSoundSettingsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetSoundSettingsIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new AccountPreferencesOutgoingPacket
		{
			UIVolume = user.Settings!.UiVolume,
			FurniVolume = user.Settings!.FurniVolume,
			TraxVolume = user.Settings!.TraxVolume,
			FreeFlowChatDisabled = false,
			RoomInvitesIgnored = false,
			RoomCameraFollowDisabled = false,
			UIFlags = 3,
			PreferredChatStyle = 0
		});
	}
}
