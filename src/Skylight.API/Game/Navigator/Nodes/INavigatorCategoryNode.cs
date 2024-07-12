using System.Collections.Immutable;

namespace Skylight.API.Game.Navigator.Nodes;

public interface INavigatorCategoryNode : INavigatorNode
{
	public new INavigatorCategoryNode? Parent { get; }

	public ImmutableArray<INavigatorNode> Children { get; }

	INavigatorNode? INavigatorNode.Parent => this.Parent;
}
