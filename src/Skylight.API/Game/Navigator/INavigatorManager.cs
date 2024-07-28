using System.Diagnostics.CodeAnalysis;
using Skylight.API.Collections.Cache;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms;

namespace Skylight.API.Game.Navigator;

public interface INavigatorManager : INavigatorSnapshot, ILoadableService<INavigatorSnapshot>
{
	public bool TryGetNode(int nodeId, [NotNullWhen(true)] out IServiceValue<INavigatorNode>? node);

	public ValueTask<IRoomInfo?> GetRoomDataAsync(int roomId, CancellationToken cancellationToken = default);
	public ValueTask<ICacheValue<IRoomInfo>?> GetRoomDataUnsafeAsync(int roomId, CancellationToken cancellationToken = default);

	//TODO: Figure out better shape for this
	public void PushRoomActivity(int roomId, int activity);
}
