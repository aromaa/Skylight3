namespace Skylight.API.Game.Permissions;

public interface IPermissionRank : IPermissionSubject
{
	public int Weight { get; }
}
