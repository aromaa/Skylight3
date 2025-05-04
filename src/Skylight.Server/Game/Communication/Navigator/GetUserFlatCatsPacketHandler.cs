using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetUserFlatCatsPacketHandler<T>(INavigatorManager navigatorManager) : UserPacketHandler<T>
	where T : IGetUserFlatCatsIncomingPacket
{
	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(async client =>
		{
			INavigatorSnapshot navigator = await this.navigatorManager.GetAsync().ConfigureAwait(false);

			List<FlatCategoryData> cats = [];
			foreach (INavigatorNode node in navigator.Nodes)
			{
				if (node is INavigatorCategoryNode)
				{
					cats.Add(new FlatCategoryData(node.Id, node.Caption, true, false, node.Caption, string.Empty, false));
				}
			}

			client.SendAsync(new UserFlatCatsOutgoingPacket(cats));
		});
	}
}
