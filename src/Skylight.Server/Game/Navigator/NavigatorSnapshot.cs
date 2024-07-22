using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
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

	public bool TryGetNode(int id, [NotNullWhen(true)] out INavigatorNode? node) => this.cache.Nodes.TryGetValue(id, out node);
	public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.cache.Layouts.TryGetValue(id, out layout);

	private readonly struct Cache(FrozenDictionary<string, IRoomLayout> layouts, FrozenDictionary<int, INavigatorNode> nodes)
	{
		internal FrozenDictionary<string, IRoomLayout> Layouts { get; } = layouts;
		internal FrozenDictionary<int, INavigatorNode> Nodes { get; } = nodes;
	}

	private readonly struct Holders(FrozenDictionary<int, ServiceValue<INavigatorNode>> nodes)
	{
		internal FrozenDictionary<int, ServiceValue<INavigatorNode>> Nodes { get; } = nodes;
	}
}
