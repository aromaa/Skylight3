using Net.Communication.Attributes;
using Skylight.API.Game.Purse;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Incoming.Purse;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Purse;

namespace Skylight.Server.Game.Communication.Purse;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetCreditsInfoPacketHandler<T>(IRegistryHolder registryHolder) : UserPacketHandler<T>
	where T : IGetCreditsInfoIncomingPacket
{
	private readonly IRegistryHolder registryHolder = registryHolder;

	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new CreditBalanceOutgoingPacket(CurrencyTypes.Credits.TryGet(this.registryHolder, out ISimpleCurrencyType? creditsCurrency)
			? user.Purse.GetBalance(creditsCurrency.Value)
			: 0));
	}
}
