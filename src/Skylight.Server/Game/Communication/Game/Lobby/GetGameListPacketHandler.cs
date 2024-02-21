using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Game.Lobby;
using Skylight.Protocol.Packets.Incoming.Game.Lobby;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Game.Lobby;

namespace Skylight.Server.Game.Communication.Game.Lobby;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetGameListPacketHandler<T> : UserPacketHandler<T>
	where T : IGetGameListIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new GameListOutgoingPacket(
		[
			new GameData(3, "basejump", "68bbd2", string.Empty, "/c_images/gamecenter_basejump/")
		]));
	}
}
