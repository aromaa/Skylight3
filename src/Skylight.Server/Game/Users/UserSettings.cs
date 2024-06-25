using Skylight.API.Game.Users;
using Skylight.Domain.Users;

namespace Skylight.Server.Game.Users;

internal sealed class UserSettings : IUserSettings
{
	public int HomeRoomId { get; set; }

	internal UserSettings(UserSettingsEntity? entity)
	{
		this.HomeRoomId = entity?.HomeRoomId ?? 0;
	}
}
