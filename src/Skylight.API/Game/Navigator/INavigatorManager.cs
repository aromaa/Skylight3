using Skylight.API.DependencyInjection;
using Skylight.API.Game.Rooms;

namespace Skylight.API.Game.Navigator;

public interface INavigatorManager : INavigatorSnapshot, ILoadableService<INavigatorSnapshot>
{
	public ValueTask<IRoomInfo?> GetRoomDataAsync(int roomId, CancellationToken cancellationToken = default);
}
