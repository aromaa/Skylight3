namespace Skylight.API.Game.Rooms;

public interface IRoomTypeManagerBuilder<T>
	where T : IRoomTypeManager
{
	public IRoomTypeManagerBuilder<T> AddTypeUnsafe<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> type);
}
