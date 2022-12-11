using Skylight.API.Game.Furniture;
using Skylight.API.Game.Users;

namespace Skylight.API.Game.Rooms.Items;

public interface IRoomItem : IFurnitureItem<IFurniture>
{
	public IRoom Room { get; }

	public int Id { get; }

	public IUserInfo Owner { get; }

	public void OnPlace();
	public void OnRemove();
}
