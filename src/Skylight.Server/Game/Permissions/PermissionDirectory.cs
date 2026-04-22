using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal abstract class PermissionDirectory<T>(PermissionManager manager, string id) : IPermissionDirectory<T>
{
	private readonly PermissionManager manager = manager;

	public string Id { get; } = id;
	public abstract IPermissionSubject Defaults { get; }

	public abstract ValueTask<IPermissionSubject?> GetSubjectAsync(T identifier);

	public IPermissionSubjectReference<T> CreateSubjectReference(T identifier) => new PermissionSubjectReference<T>(this.manager, this.Id, identifier);
}
