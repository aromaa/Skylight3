namespace Skylight.API.Game.Rooms;

public interface IRoomTask
{
	public void Execute(IRoom room);
}

public interface IRoomTask<T>
{
	public T Execute(IRoom room);
}

public interface IAsyncRoomTask<T>
{
	public ValueTask<T> Execute(IRoom room);
}
