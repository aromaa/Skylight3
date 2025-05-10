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
internal sealed class ListenOfficialRoomsPacketHandler<T>(INavigatorManager navigatorManager) : UserPacketHandler<T>
	where T : IListenOfficialRoomsIncomingPacket
{
	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		List<NavigatorNodeData> nodes = [];
		foreach (INavigatorNode node in this.navigatorManager.Nodes)
		{
			if (node is INavigatorPublicRoomNode publicRoom)
			{
				nodes.Add(new NavigatorPublicRoomNode(publicRoom.Id, publicRoom.Parent?.Id ?? 0, publicRoom.Caption, 0, 0, publicRoom.Name, publicRoom.InstanceId, publicRoom.WorldId, string.Empty, 0, string.Join(',', publicRoom.Casts)));
			}
		}

		user.SendAsync(new OfficialRoomsOutgoingPacket(0, nodes));
	}
}
