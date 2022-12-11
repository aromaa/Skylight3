using Skylight.Domain.Rooms.Layout;

namespace Skylight.Domain.Items;

public class PublicRoomItemEntity
{
	public int Id { get; init; }

	public string LayoutId { get; set; } = null!;
	public RoomLayoutEntity? Layout { get; set; }

	public string SpriteId { get; set; } = null!;

	public int X { get; set; }
	public int Y { get; set; }
	public int Z { get; set; }

	public int Direction { get; set; }
}
