namespace Skylight.API.Game.Rooms;

public interface IRoomTask<T>
{
	public T Execute(IRoom room);
}
