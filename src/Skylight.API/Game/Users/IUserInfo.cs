using Skylight.API.Game.Figure;

namespace Skylight.API.Game.Users;

public interface IUserInfo : IUserInfoView, IUserInfoEvents
{
	public new FigureAvatar Avatar { get; set; }
	public new string Motto { get; set; }

	public new DateTime LastOnline { get; set; }

	public IUserInfoEvents Events(IUserInfoView view);
}
