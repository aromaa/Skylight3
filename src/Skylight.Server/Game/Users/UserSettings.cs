using Skylight.API.Game.Users;
using Skylight.Domain.Users;

internal sealed class UserSettings : IUserSettings
{
	public int UserId { get; }
	public int HomeRoomId { get; set; }

	internal UserSettings(UserSettingsEntity entity)
	{
		this.UserId = entity.UserId;

		this.HomeRoomId = entity.Home;
	}
}
