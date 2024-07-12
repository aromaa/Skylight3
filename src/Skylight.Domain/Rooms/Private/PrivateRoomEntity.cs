using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Users;

namespace Skylight.Domain.Rooms.Private;

public class PrivateRoomEntity
{
	public int Id { get; init; }

	public int OwnerId { get; set; }
	public UserEntity? Owner { get; set; }

	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;

	public string LayoutId { get; set; } = null!;
	public RoomLayoutEntity? Layout { get; set; }

	public int CategoryId { get; set; }
	public NavigatorCategoryNodeEntity? Category { get; set; }

	public int UsersMax { get; set; }
}
