using System.Diagnostics.CodeAnalysis;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorSnapshot
{
	internal bool TryGetNode<T>(int nodeId, [NotNullWhen(true)] out IServiceValue<T>? node)
		where T : class, INavigatorNode
	{
		if (this.holders.Nodes.TryGetValue(nodeId, out IServiceValue<INavigatorNode>? holder) && holder is ServiceValue<T> holderOfT)
		{
			node = holderOfT;
			return true;
		}

		node = null;
		return false;
	}
}
