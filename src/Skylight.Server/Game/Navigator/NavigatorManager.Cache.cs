using System.Collections.Frozen;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Server.Game.Rooms;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Navigator;

internal partial class NavigatorManager
{
	private sealed class Cache
	{
		internal FrozenDictionary<string, IRoomLayout> Layouts { get; }
		internal FrozenDictionary<int, IRoomFlatCat> FlatCats { get; }

		private Cache(Dictionary<string, IRoomLayout> layouts, Dictionary<int, IRoomFlatCat> flatCats)
		{
			this.Layouts = layouts.ToFrozenDictionary();
			this.FlatCats = flatCats.ToFrozenDictionary();
		}

		internal static Builder CreateBuilder() => new();

		internal sealed class Builder
		{
			private readonly Dictionary<string, RoomLayoutEntity> layouts;
			private readonly Dictionary<int, RoomFlatCatEntity> flatCats;

			internal Builder()
			{
				this.layouts = [];
				this.flatCats = [];
			}

			internal void AddLayout(RoomLayoutEntity layout)
			{
				this.layouts.Add(layout.Id, layout);
			}

			internal void AddFlatCat(RoomFlatCatEntity flatCat)
			{
				this.flatCats.Add(flatCat.Id, flatCat);
			}

			internal Cache ToImmutable()
			{
				Dictionary<string, IRoomLayout> layouts = [];
				Dictionary<int, IRoomFlatCat> flatCats = [];

				foreach (RoomLayoutEntity entity in this.layouts.Values)
				{
					RoomLayout layout = new(entity);

					layouts.Add(layout.Id, layout);
				}

				foreach (RoomFlatCatEntity entity in this.flatCats.Values)
				{
					RoomFlatCat flatCat = new(entity.Id, entity.Caption);

					flatCats.Add(flatCat.Id, flatCat);
				}

				return new Cache(layouts, flatCats);
			}
		}
	}
}
