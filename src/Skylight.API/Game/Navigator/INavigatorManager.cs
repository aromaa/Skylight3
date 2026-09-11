using Skylight.API.Collections.Cache;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Rooms.Private;

namespace Skylight.API.Game.Navigator;

public interface INavigatorManager : ILoadableService<INavigatorSnapshot>
{
	public ValueTask<IPrivateRoomInfo?> GetPrivateRoomInfoAsync(int roomId, CancellationToken cancellationToken = default);
	public ValueTask<ICacheReference<IPrivateRoomInfo>?> GetPrivateRoomInfoUnsafeAsync(int roomId, CancellationToken cancellationToken = default);
}
