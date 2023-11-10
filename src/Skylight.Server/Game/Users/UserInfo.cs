using Skylight.API.Game.Users;
using Skylight.Domain.Users;

namespace Skylight.Server.Game.Users;

internal sealed class UserInfo : IUserProfile
{
	public int Id { get; }

	public string Username { get; set; }
	public string Figure { get; set; }
	public string Gender { get; set; }
	public string Motto { get; set; }

	public DateTime LastOnline { get; set; }

	internal UserInfo(UserEntity entity)
	{
		this.Id = entity.Id;

		this.Username = entity.Username;
		this.Figure = entity.Figure;
		this.Gender = entity.Gender;
		this.Motto = entity.Motto;

		this.LastOnline = entity.LastOnline;
	}
}
