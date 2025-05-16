using Net.Communication.Attributes;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Purse;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Purse;

namespace Skylight.Server.Game.Communication.Purse;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetCreditsInfoPacketHandler<T> : UserPacketHandler<T>
	where T : IGetCreditsInfoIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		int creditsBalance = user.Purse.GetBalance(CurrencyKeys.Credits);
		user.SendAsync(new CreditBalanceOutgoingPacket((int)creditsBalance));
	}
}
