using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Users;

namespace Skylight.Domain.Rooms.Private;

public class PrivateRoomEntity
{
	public int Id { get; init; }

	public int OwnerId { get; set; }
	public UserEntity? Owner { get; set; }

	public string LayoutId { get; set; } = null!;
	public RoomLayoutEntity? Layout { get; set; }

	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;

	public int CategoryId { get; set; }
	public NavigatorCategoryNodeEntity? Category { get; set; }

	public string[] Tags { get; set; } = null!;

	public PrivateRoomEntryMode EntryMode { get; set; }
	public string? Password { get; set; }
	public int UsersMax { get; set; }

	public PrivateRoomTradeMode TradeMode { get; set; }
	public bool WalkThrough { get; set; }
	public bool AllowPets { get; set; }
	public bool AllowPetsToEat { get; set; }

	public bool HideWalls { get; set; }
	public int FloorThickness { get; set; }
	public int WallThickness { get; set; }
}
