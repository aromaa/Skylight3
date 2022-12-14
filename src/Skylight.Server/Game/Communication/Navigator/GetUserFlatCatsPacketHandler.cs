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
internal sealed class GetUserFlatCatsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetUserFlatCatsIncomingPacket
{
	private readonly INavigatorManager navigatorManager;

	public GetUserFlatCatsPacketHandler(INavigatorManager navigatorManager)
	{
		this.navigatorManager = navigatorManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		List<FlatCategoryData> cats = new();

		foreach (IRoomFlatCat flatCat in this.navigatorManager.FlatCats)
		{
			cats.Add(new FlatCategoryData(flatCat.Id, flatCat.Caption, true, false, flatCat.Caption, string.Empty, false));
		}

		user.SendAsync(new UserFlatCatsOutgoingPacket(cats));
	}
}
