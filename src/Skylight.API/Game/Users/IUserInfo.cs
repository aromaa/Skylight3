using Skylight.API.Game.Figure;

namespace Skylight.API.Game.Users;

public interface IUserInfo
{
	public int Id { get; }

	public string Username { get; set; }

	public FigureAvatar Avatar { get; set; }
	public string Motto { get; set; }

	public DateTime LastOnline { get; set; }
}
