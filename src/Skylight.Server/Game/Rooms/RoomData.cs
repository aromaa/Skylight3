using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomData : IRoomInfo
{
	public int Id { get; }

	public string Name { get; }

	public IUserInfo Owner { get; }

	public IRoomLayout Layout { get; }

	public RoomData(RoomEntity entity, IUserInfo owner, IRoomLayout layout)
	{
		this.Id = entity.Id;

		this.Name = entity.Name;

		this.Owner = owner;

		this.Layout = layout;
	}
}
