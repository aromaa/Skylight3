using Skylight.Domain.Items;
using Skylight.Domain.Users;

namespace Skylight.Domain.Rooms.Sound;

public class SongEntity
{
	public int Id { get; init; }

	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public int ItemId { get; set; }
	public FloorItemEntity? Item { get; set; }

	public string Name { get; set; } = null!;

	public int Length { get; set; }
	public string Data { get; set; } = null!;
}
