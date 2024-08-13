using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Navigator;

internal sealed partial class NavigatorSnapshot : VersionedServiceSnapshot, INavigatorSnapshot
{
	private readonly Cache cache;
	private readonly Holders holders;

	private NavigatorSnapshot(in Cache cache, in Holders holders)
	{
		this.cache = cache;
		this.holders = holders;
	}

	public IEnumerable<INavigatorNode> Nodes => this.cache.Nodes.Values;

	public bool TryGetNode<T>(int id, [NotNullWhen(true)] out T? node)
		where T : class, INavigatorNode
	{
		if (this.cache.Nodes.TryGetValue(id, out INavigatorNode? value) && value is T valueOfT)
		{
			node = valueOfT;
			return true;
		}

		node = null;
		return false;
	}

	public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.cache.Layouts.TryGetValue(id, out layout);

	private readonly struct Cache(FrozenDictionary<string, IRoomLayout> layouts, FrozenDictionary<int, INavigatorNode> nodes)
	{
		internal FrozenDictionary<string, IRoomLayout> Layouts { get; } = layouts;
		internal FrozenDictionary<int, INavigatorNode> Nodes { get; } = nodes;
	}

	private readonly struct Holders(FrozenDictionary<int, IServiceValue<INavigatorNode>> nodes)
	{
		internal FrozenDictionary<int, IServiceValue<INavigatorNode>> Nodes { get; } = nodes;
	}
}
