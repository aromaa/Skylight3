namespace Skylight.Domain.Users;

public class UserWardrobeSlotEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }

	public int SlotId { get; init; }

	public string Gender { get; set; } = null!;
	public string Figure { get; set; } = null!;
}
