using Skylight.API.Game.Rooms;

namespace Skylight.API.Game.Navigator;

public interface INavigatorManager : INavigatorSnapshot
{
	public INavigatorSnapshot Current { get; }

	public Task<INavigatorSnapshot> LoadAsync(CancellationToken cancellationToken = default);

	public ValueTask<IRoomInfo?> GetRoomDataAsync(int roomId, CancellationToken cancellationToken = default);
}
