using Skylight.Domain.Furniture;
using Skylight.Domain.Rooms;
using Skylight.Domain.Users;

namespace Skylight.Domain.Items;

public class FloorItemEntity
{
	public int Id { get; init; }

	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public int FurnitureId { get; set; }
	public FloorFurnitureEntity? Furniture { get; set; }

	public int? RoomId { get; set; }
	public RoomEntity? Room { get; set; }

	public int X { get; set; }
	public int Y { get; set; }
	public double Z { get; set; }

	public int Direction { get; set; }

	public FloorItemDataEntity? Data { get; set; }
}
