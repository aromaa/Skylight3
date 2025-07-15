namespace Skylight.API.Game.Permissions;

public interface IPermissionSubjectReference
{
	public string Directory { get; }

	public ValueTask<IPermissionSubject?> Resolve();
}

public interface IPermissionSubjectReference<out T> : IPermissionSubjectReference
{
	public T Identifier { get; }
}
