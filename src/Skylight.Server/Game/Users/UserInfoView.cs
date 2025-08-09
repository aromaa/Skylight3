using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Users;

internal sealed class UserInfoView : IUserInfoView
{
	public int Id { get; }
	public string Username { get; internal init; }
	public FigureAvatar Avatar { get; internal init; }
	public string Motto { get; internal init; }
	public DateTime LastOnline { get; internal init; }

	internal UserInfoView(int id, string username, FigureAvatar avatar, string motto, DateTime lastOnline)
	{
		this.Id = id;
		this.Username = username;
		this.Avatar = avatar;
		this.Motto = motto;
		this.LastOnline = lastOnline;
	}

	internal UserInfoView(UserInfoView other)
		: this(other.Id, other.Username, other.Avatar, other.Motto, other.LastOnline)
	{
	}

	public IUserInfoView Snapshot => this;
}
