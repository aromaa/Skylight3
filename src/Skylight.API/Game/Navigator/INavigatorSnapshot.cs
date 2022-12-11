using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;

namespace Skylight.API.Game.Navigator;

public interface INavigatorSnapshot
{
	public IEnumerable<IRoomFlatCat> FlatCats { get; }

	public bool TryGetLayout(string layoutId, [NotNullWhen(true)] out IRoomLayout? layout);
	public bool TryGetFlatCat(int flatCatId, [NotNullWhen(true)] out IRoomFlatCat? flatCat);
}
