using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomTypeManagerBuilder<T>(Type type) : IRoomTypeManagerBuilder<T>
	where T : IRoomTypeManager
{
	internal Type Type { get; } = type;

	internal List<IRoomType> Types { get; } = [];

	public IRoomTypeManagerBuilder<T> AddTypeUnsafe<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> type)
	{
		if (!typeof(T).IsAssignableTo(typeof(IRoomTypeManager<TInstance, TInfo, TId>)))
		{
			throw new ArgumentException("The room type manager is not compatible with the specified room type.", nameof(type));
		}

		this.Types.Add(type);

		return this;
	}
}
