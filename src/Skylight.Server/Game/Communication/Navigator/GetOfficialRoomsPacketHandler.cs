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
internal sealed class GetOfficialRoomsPacketHandler<T>(INavigatorManager navigatorManager) : UserPacketHandler<T>
	where T : IGetOfficialRoomsIncomingPacket
{
	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		int nodeMask = packet.NodeMask;

		if (!this.navigatorManager.TryGetNode(packet.NodeId, out INavigatorNode? node))
		{
			return;
		}

		List<NavigatorNodeData> nodes = [];
		if (node is INavigatorCategoryNode category)
		{
			nodes.Add(new NavigatorCategoryNodeData(category.Id, category.Parent?.Id ?? 0, category.Caption, 0, 0));

			foreach (INavigatorNode childNode in category.Children)
			{
				if (childNode is INavigatorCategoryNode childCategory)
				{
					nodes.Add(new NavigatorCategoryNodeData(childCategory.Id, childCategory.Parent?.Id ?? 0, childCategory.Caption, 0, 0));
				}
				else if (childNode is INavigatorPublicRoomNode publicRoom)
				{
					nodes.Add(new NavigatorPublicRoomNode(publicRoom.Id, publicRoom.Parent?.Id ?? 0, publicRoom.Caption, 0, 0, publicRoom.Name, publicRoom.InstanceId, publicRoom.WorldId, string.Empty, 0, string.Join(',', publicRoom.Casts)));
				}
				else
				{
					throw new NotSupportedException();
				}
			}
		}
		else if (node is INavigatorPublicRoomNode publicRoom)
		{
			nodes.Add(new NavigatorPublicRoomNode(publicRoom.Id, publicRoom.Parent?.Id ?? 0, publicRoom.Caption, 0, 0, publicRoom.Name, publicRoom.InstanceId, publicRoom.WorldId, string.Empty, 0, string.Join(',', publicRoom.Casts)));
		}
		else
		{
			return;
		}

		user.SendAsync(new OfficialRoomsOutgoingPacket(nodeMask, nodes));
	}
}
