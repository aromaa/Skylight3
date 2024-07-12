using System.Collections.Immutable;
using Skylight.API.Game.Navigator.Nodes;

namespace Skylight.Server.Game.Navigator.Nodes;

internal sealed class NavigatorCategoryNode(INavigatorCategoryNode? parent, int id, string caption) : NavigatorNode(parent, id, caption), INavigatorCategoryNode
{
	public override INavigatorCategoryNode? Parent => (INavigatorCategoryNode?)base.Parent;

	public ImmutableArray<INavigatorNode> Children { get; internal set; }
}
