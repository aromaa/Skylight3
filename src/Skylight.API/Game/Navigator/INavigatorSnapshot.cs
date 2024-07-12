using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.API.Game.Navigator;

public interface INavigatorSnapshot
{
	public IEnumerable<INavigatorNode> Nodes { get; }

	public bool TryGetNode(int nodeId, [NotNullWhen(true)] out INavigatorNode? node);
	public bool TryGetLayout(string layoutId, [NotNullWhen(true)] out IRoomLayout? layout);
}
