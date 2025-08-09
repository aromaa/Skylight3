namespace Skylight.API.Game.Users;

public interface IUserSettings
{
	public int HomeRoomId { get; set; }
	public int UiVolume { get; set; }
	public int FurniVolume { get; set; }
	public int TraxVolume { get; set; }
}
