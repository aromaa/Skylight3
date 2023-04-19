namespace Skylight.API.Game.Users;
internal interface IUserSettings
{
	public int UserId { get; }
	public int Home { get; set; }
}
