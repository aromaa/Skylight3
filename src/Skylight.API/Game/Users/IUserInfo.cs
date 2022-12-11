namespace Skylight.API.Game.Users;

public interface IUserInfo
{
	public int Id { get; }

	public string Username { get; set; }
	public string Figure { get; set; }
	public string Gender { get; set; }

	public DateTimeOffset LastOnline { get; set; }
}
