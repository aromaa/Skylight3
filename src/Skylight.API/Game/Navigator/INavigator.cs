using System.Diagnostics.CodeAnalysis;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.API.Game.Navigator;

public interface INavigator
{
	public IEnumerable<INavigatorNode> Nodes { get; }

	public INavigatorCategoryNode PublicRoomsRootNode { get; }
	public INavigatorCategoryNode PrivateRoomsRootNode { get; }

	public bool TryGetNode<T>(int nodeId, [NotNullWhen(true)] out T? node)
		where T : class, INavigatorNode;

	public bool TryGetNode<T>(int nodeId, [NotNullWhen(true)] out IServiceValue<T>? node)
		where T : class, INavigatorNode;

	public bool TryGetLayout(string layoutId, [NotNullWhen(true)] out IRoomLayout? layout);
}
