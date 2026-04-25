using System.Diagnostics.CodeAnalysis;
using Skylight.API.Collections.Cache;

namespace Skylight.API.Game.Rooms;

public interface IRoomManager
{
	public IEnumerable<IRoom> LoadedRooms { get; }

	public IEnumerable<TInstance> GetLoadedInstances<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> roomType);

	public ValueTask<ICacheReference<TInstance>?> GetInstanceAsync<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> roomType, TId roomId, CancellationToken cancellationToken = default);

	public bool TryGetInstance<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> roomType, TId roomId, [NotNullWhen(true)] out TInstance? instance);
}
