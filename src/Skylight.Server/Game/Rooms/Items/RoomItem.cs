using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Rooms.Items;

internal abstract class RoomItem<T>(IPrivateRoom room, RoomItemId id, IUserInfo owner, T furniture) : IRoomItem
	where T : IFurniture
{
	public IPrivateRoom Room { get; } = room;

	public RoomItemId Id { get; } = id;

	public IUserInfo Owner { get; } = owner;

	protected readonly T furniture = furniture;

	public abstract int StripId { get; }

	public IFurniture Furniture => this.furniture;

	public virtual void OnPlace()
	{
	}

	public virtual void OnRemove()
	{
	}
}
