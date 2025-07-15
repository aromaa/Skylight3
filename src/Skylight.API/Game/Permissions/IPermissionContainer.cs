using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Game.Permissions;

public interface IPermissionContainer
{
	public IEnumerable<IPermissionSubjectReference> Parents { get; }

	public bool TryGetPermission(string permission, out bool value);
	public bool TryGetEntitlement(string entitlement, [NotNullWhen(true)] out string? value);

	public ValueTask<bool> SetPermissionAsync(string permission, bool value);
	public ValueTask<bool> RemovePermissionAsync(string permission);

	public ValueTask<bool> SetEntitlementAsync(string entitlement, string value);
	public ValueTask<bool> RemoveEntitlementAsync(string entitlement);

	public ValueTask<bool> AddParentAsync(IPermissionSubjectReference reference);
	public ValueTask<bool> RemoveParentAsync(IPermissionSubjectReference reference);
}
