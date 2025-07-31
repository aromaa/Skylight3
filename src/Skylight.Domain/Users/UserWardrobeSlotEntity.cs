using Skylight.Domain.Figure;

namespace Skylight.Domain.Users;

public class UserWardrobeSlotEntity
{
	public int UserId { get; init; }
	public UserEntity? User { get; set; }

	public int SlotId { get; init; }

	public FigureSexType Sex { get; set; }

	public List<UserWardrobeSlotFigureEntity>? FigureSets { get; set; }
}
