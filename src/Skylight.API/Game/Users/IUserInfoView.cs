using Skylight.API.Game.Figure;

namespace Skylight.API.Game.Users;

public interface IUserInfoView
{
	public int Id { get; }

	public string Username { get; }

	public FigureAvatar Avatar { get; }
	public string Motto { get; }

	public DateTime LastOnline { get; }

	public IUserInfoView Snapshot { get; }
}
