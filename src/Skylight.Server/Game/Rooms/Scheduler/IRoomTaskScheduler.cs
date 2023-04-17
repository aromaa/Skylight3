namespace Skylight.Server.Game.Rooms.Scheduler;

internal interface IRoomTaskScheduler<T>
{
	public T Execute(Room room);
	public T CreateTask(Room room);
}
