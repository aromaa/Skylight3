namespace Skylight.API.Game.Rooms;

public interface IRoomManager
{
	public ValueTask<IRoom?> GetRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
