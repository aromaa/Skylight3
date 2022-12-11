using Skylight.Domain.Items;
using Skylight.Domain.Rooms;

namespace Skylight.Domain.Users;

public class UserEntity
{
	public int Id { get; init; }

	public string Username { get; set; } = null!;
	public string Figure { get; set; } = null!;
	public string Gender { get; set; } = null!;

	public DateTimeOffset LastOnline { get; set; }

	public List<FloorItemEntity>? FloorItems { get; set; }
	public List<WallItemEntity>? WallItems { get; set; }

	public List<RoomEntity>? Rooms { get; set; }
}
