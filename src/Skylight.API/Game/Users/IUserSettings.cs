namespace Skylight.API.Game.Users;

public interface IUserSettings
{
	public int UserId { get; }
	public int Home { get; set; }
}
