using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionDirectory<T>(PermissionManager manager, string id, Func<IPermissionSubject>? defaults, Func<T, ValueTask<IPermissionSubject?>> fetcher) : IPermissionDirectory<T>
{
	private readonly PermissionManager manager = manager;

	public string Id { get; } = id;
	public IPermissionSubject Defaults => defaults?.Invoke() ?? new PermissionSubject<string>((PermissionDirectory<string>)this.manager.Defaults.Directory, this.Id);

	public ValueTask<IPermissionSubject?> GetSubjectAsync(T identifier) => fetcher(identifier);
	public IPermissionSubjectReference<T> CreateSubjectReference(T identifier) => new PermissionSubjectReference<T>(this.manager, this.Id, identifier);
}
