namespace Skylight.API.Game.Rooms;

public static class RoomTypeManagerBuilderExtensions
{
	public static IRoomTypeManagerBuilder<T> AddType<T, TInstance, TInfo, TId>(this IRoomTypeManagerBuilder<T> builder, IRoomType<TInstance, TInfo, TId> type)
		where T : IRoomTypeManager<TInstance, TInfo, TId> => builder.AddTypeUnsafe(type);
}
