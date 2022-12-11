namespace Skylight.API.Game.Users;

public interface IUserManager
{
	public ValueTask<IUserInfo?> GetUserInfoAsync(int userId, CancellationToken cancellationToken = default);
	public ValueTask<IUserProfile?> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default);

	public Task<IUserProfile?> LoadUserProfileAsync(int userId, CancellationToken cancellationToken = default);
}
