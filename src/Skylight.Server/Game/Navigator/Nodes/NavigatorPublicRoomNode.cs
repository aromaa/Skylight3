using System.Collections.Immutable;
using Skylight.API.Game.Navigator.Nodes;

namespace Skylight.Server.Game.Navigator.Nodes;

internal sealed class NavigatorPublicRoomNode(INavigatorNode? parent, int id, string caption, string name, int instanceId, int worldId, ImmutableArray<string> casts)
	: NavigatorNode(parent, id, caption), INavigatorPublicRoomNode
{
	public string Name { get; } = name;

	public int InstanceId { get; } = instanceId;
	public int WorldId { get; } = worldId;

	public ImmutableArray<string> Casts { get; } = casts;
}
