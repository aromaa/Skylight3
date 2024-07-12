namespace Skylight.Domain.Rooms.Public;

public class PublicRoomEntity
{
	public int Id { get; init; }

	public string Name { get; set; } = null!;
	public string[] Casts { get; set; } = null!;

	public int UsersMax { get; set; }
}
