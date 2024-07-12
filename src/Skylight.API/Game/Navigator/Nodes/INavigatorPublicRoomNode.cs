using System.Collections.Immutable;

namespace Skylight.API.Game.Navigator.Nodes;

public interface INavigatorPublicRoomNode : INavigatorNode
{
	public string Name { get; }

	public int InstanceId { get; }
	public int WorldId { get; }

	public ImmutableArray<string> Casts { get; }
}
