using Skylight.Domain.Figure;

namespace Skylight.Domain.Users;

public class UserFigureColorEntity
{
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public int SetTypeId { get; set; }
	public FigureSetTypeEntity? SetType { get; set; }

	public int Index { get; set; }

	public int ColorId { get; set; }
	public FigureColorPaletteColorEntity? Color { get; set; }
}
