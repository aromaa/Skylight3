using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionDirectory<T>(PermissionManager manager, string id, Func<IPermissionSubject>? defaults, Func<T, ValueTask<IPermissionSubject?>> fetcher) : IPermissionDirectory<T>
{
	private readonly PermissionManager manager = manager;

	public string Id { get; } = id;
	public IPermissionSubject Defaults => defaults?.Invoke() ?? new PermissionSubject(this);

	public ValueTask<IPermissionSubject?> GetSubjectAsync(T identifier) => fetcher(identifier);
}
