using Skylight.Domain.Figure;

namespace Skylight.Domain.Users;

public class UserWardrobeSlotFigureEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public int SlotId { get; set; }

	public int SetTypeId { get; set; }
	public FigureSetTypeEntity? SetType { get; set; }

	public int SetId { get; set; }
	public FigureSetEntity? Set { get; set; }

	public List<UserWardrobeSlotFigureColorEntity>? Colors { get; set; }
}
