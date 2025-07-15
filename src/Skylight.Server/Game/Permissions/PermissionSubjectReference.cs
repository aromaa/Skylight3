using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionSubjectReference<T>(PermissionManager permissionManager, string directory, T identifier) : IPermissionSubjectReference<T>
{
	private readonly PermissionManager permissionManager = permissionManager;

	public string Directory { get; } = directory;
	public T Identifier { get; } = identifier;

	public async ValueTask<IPermissionSubject?> Resolve()
	{
		IPermissionDirectory? directory = await this.permissionManager.GetDirectoryAsync(this.Directory).ConfigureAwait(false);
		if (directory is IPermissionDirectory<T> typedDirectory)
		{
			return await typedDirectory.GetSubjectAsync(this.Identifier).ConfigureAwait(false);
		}

		return null;
	}
}
