using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomData(RoomEntity entity, IUserInfo owner, IRoomLayout layout) : IRoomInfo
{
	public int Id { get; } = entity.Id;

	public string Name { get; } = entity.Name;

	public IUserInfo Owner { get; } = owner;

	public IRoomLayout Layout { get; } = layout;
}
