using System.Diagnostics.CodeAnalysis;
using Skylight.API.Collections.Cache;

namespace Skylight.API.Game.Rooms;

public interface IRoomTypeManager
{
	public IEnumerable<IRoom> LoadedRooms { get; }
}

public interface IRoomTypeManager<TInstance, TInfo, TId> : IRoomTypeManager
{
	public IEnumerable<TInstance> LoadedInstances { get; }

	public ValueTask<ICacheReference<TInstance>?> GetInstanceAsync(TId roomId, CancellationToken cancellationToken = default);

	public bool TryGetInstance(TId roomId, [NotNullWhen(true)] out TInstance? instance);
}
