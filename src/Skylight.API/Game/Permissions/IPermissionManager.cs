using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Permissions;

public interface IPermissionManager : ILoadableService
{
	public ValueTask<IPermissionSubject> GetDefaultsAsync(CancellationToken cancellationToken = default);

	public ValueTask<IPermissionDirectory?> GetDirectoryAsync(string identifier, CancellationToken cancellationToken = default);

	public ValueTask<IPermissionDirectory<string>> GetDefaultsDirectoryAsync(CancellationToken cancellationToken = default);
	public ValueTask<IPermissionDirectory<string>> GetRanksDirectoryAsync(CancellationToken cancellationToken = default);
	public ValueTask<IPermissionDirectory<int>> GetUserDirectoryAsync(CancellationToken cancellationToken = default);
}
