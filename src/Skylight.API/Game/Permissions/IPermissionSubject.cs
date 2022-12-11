namespace Skylight.API.Game.Permissions;

public interface IPermissionSubject
{
	public bool HasPermissions(string permission);
}
