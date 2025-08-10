using System.Diagnostics.CodeAnalysis;
using Skylight.API.Collections.Cache;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Private;

namespace Skylight.API.Game.Navigator;

public interface INavigatorManager : INavigator, ILoadableService<INavigatorSnapshot>
{
	public bool TryGetNode<T>(int nodeId, [NotNullWhen(true)] out IServiceValue<T>? node)
		where T : class, INavigatorNode;

	public ValueTask<IPrivateRoomInfo?> GetPrivateRoomInfoAsync(int roomId, CancellationToken cancellationToken = default);
	public ValueTask<ICacheReference<IPrivateRoomInfo>?> GetPrivateRoomInfoUnsafeAsync(int roomId, CancellationToken cancellationToken = default);
}
