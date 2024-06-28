namespace Skylight.API.Game.Rooms;

public interface IRoomManager
{
	public IEnumerable<IRoom> LoadedRooms { get; }

	public ValueTask<IRoom?> GetRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
