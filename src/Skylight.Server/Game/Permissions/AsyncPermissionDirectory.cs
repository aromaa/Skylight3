using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class AsyncPermissionDirectory<T>(PermissionManager manager, string id, IPermissionSubject defaults, Func<T, ValueTask<IPermissionSubject?>> fetcher) : PermissionDirectory<T>(manager, id)
{
	private readonly Func<T, ValueTask<IPermissionSubject?>> fetcher = fetcher;

	public override IPermissionSubject Defaults { get; } = defaults;

	public override ValueTask<IPermissionSubject?> GetSubjectAsync(T identifier) => this.fetcher(identifier);
}
