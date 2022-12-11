using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorManager
{
	public IEnumerable<IRoomFlatCat> FlatCats => this.snapshot.FlatCats;

	public bool TryGetFlatCat(int id, [NotNullWhen(true)] out IRoomFlatCat? flatCat) => this.snapshot.TryGetFlatCat(id, out flatCat);
	public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.snapshot.TryGetLayout(id, out layout);

	private sealed class Snapshot : INavigatorSnapshot
	{
		private readonly NavigatorManager navigatorManager;
		private readonly Cache cache;

		internal Snapshot(NavigatorManager navigatorManager, Cache cache)
		{
			this.navigatorManager = navigatorManager;
			this.cache = cache;
		}

		public IEnumerable<IRoomFlatCat> FlatCats => this.cache.FlatCats.Values;

		public bool TryGetFlatCat(int id, [NotNullWhen(true)] out IRoomFlatCat? flatCat) => this.cache.FlatCats.TryGetValue(id, out flatCat);
		public bool TryGetLayout(string id, [NotNullWhen(true)] out IRoomLayout? layout) => this.cache.Layouts.TryGetValue(id, out layout);
	}
}
