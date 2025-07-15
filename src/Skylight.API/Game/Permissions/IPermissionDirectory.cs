namespace Skylight.API.Game.Permissions;

public interface IPermissionDirectory
{
	public string Id { get; }

	public IPermissionSubject Defaults { get; }
}

public interface IPermissionDirectory<T> : IPermissionDirectory
{
	ValueTask<IPermissionSubject?> GetSubjectAsync(T identifier);
}
