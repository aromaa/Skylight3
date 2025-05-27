using Skylight.API.Game.Users;
using Skylight.Domain.Users;

namespace Skylight.Server.Game.Users;

internal sealed class UserSettings : IUserSettings
{
	public int HomeRoomId { get; set; }
	public int UiVolume { get; set; }
	public int FurniVolume { get; set; }
	public int TraxVolume { get; set; }

	internal UserSettings(UserSettingsEntity? entity)
	{
		this.HomeRoomId = entity?.HomeRoomId ?? 0;
		this.UiVolume = entity?.UiVolume ?? 75;
		this.FurniVolume = entity?.FurniVolume ?? 75;
		this.TraxVolume = entity?.TraxVolume ?? 75;
	}
}
