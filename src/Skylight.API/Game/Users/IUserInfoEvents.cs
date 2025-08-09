using Skylight.API.Events;
using Skylight.API.Game.Figure;

namespace Skylight.API.Game.Users;

public interface IUserInfoEvents
{
	public event EventHandler<ValueChangedEventArgs<FigureAvatar>> AvatarChanged;
	public event EventHandler<ValueChangedEventArgs<string>> MottoChanged;
}
