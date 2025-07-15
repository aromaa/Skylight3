using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Game.Permissions;

public interface IPermissionSubject
{
	public IPermissionDirectory Directory { get; }

	public IPermissionContainer Container { get; }
	public IPermissionContainer TransientContainer { get; }

	public bool HasPermissions(string permission) => this.TryGetPermission(permission, out bool value) && value;
	public string? Entitlement(string entitlement) => !this.TryGetEntitlement(entitlement, out string? value) ? null : value;

	public bool TryGetPermission(string permission, out bool value);
	public bool TryGetEntitlement(string entitlement, [NotNullWhen(true)] out string? value);

	public bool IsChildOf(IPermissionSubjectReference parent);
}
