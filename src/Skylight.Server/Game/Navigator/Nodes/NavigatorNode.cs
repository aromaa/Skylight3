using Skylight.API.Game.Navigator.Nodes;

namespace Skylight.Server.Game.Navigator.Nodes;

internal abstract class NavigatorNode(INavigatorNode? parent, int id, string caption) : INavigatorNode
{
	public virtual INavigatorNode? Parent { get; } = parent;

	public int Id { get; } = id;
	public string Caption { get; } = caption;
}
