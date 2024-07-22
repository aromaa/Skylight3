using System.Diagnostics.CodeAnalysis;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorSnapshot
{
	internal bool TryGetNode(int nodeId, [NotNullWhen(true)] out IServiceValue<INavigatorNode>? node)
	{
		if (this.holders.Nodes.TryGetValue(nodeId, out ServiceValue<INavigatorNode>? holder))
		{
			node = holder;
			return true;
		}

		node = null;
		return false;
	}

	private bool TryGetNode(int nodeId, [NotNullWhen(true)] out ServiceValue<INavigatorNode>? node) => this.holders.Nodes.TryGetValue(nodeId, out node);
}
