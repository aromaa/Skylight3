using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorManager
{
	public IEnumerable<INavigatorNode> Nodes => this.Current.Nodes;

	public bool TryGetNode(int id, [NotNullWhen(true)] out INavigatorNode? node) => this.Current.TryGetNode(id, out node);
	public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.Current.TryGetLayout(id, out layout);

	private sealed class Snapshot : INavigatorSnapshot
	{
		private readonly Cache cache;

		internal Snapshot(Cache cache)
		{
			this.cache = cache;
		}

		public IEnumerable<INavigatorNode> Nodes => this.cache.Nodes.Values;

		public bool TryGetNode(int id, [NotNullWhen(true)] out INavigatorNode? node) => this.cache.Nodes.TryGetValue(id, out node);
		public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.cache.Layouts.TryGetValue(id, out layout);
	}
}
