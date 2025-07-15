using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionContainer : IPermissionContainer
{
	private readonly ConcurrentDictionary<string, bool> permissions = [];
	private readonly ConcurrentDictionary<string, string> entitlements = [];

	private readonly ConcurrentDictionary<IPermissionSubjectReference, bool> parents = [];

	public IEnumerable<IPermissionSubjectReference> Parents => this.parents.Keys;

	public bool TryGetPermission(string permission, out bool value)
	{
		if (this.permissions.TryGetValue(permission, out value))
		{
			return true;
		}

		ConcurrentDictionary<string, bool>.AlternateLookup<ReadOnlySpan<char>> permissions = this.permissions.GetAlternateLookup<ReadOnlySpan<char>>();

		ReadOnlySpan<char> span = permission;
		while (!span.IsEmpty)
		{
			int split = span.LastIndexOf('.');
			if (split < 0)
			{
				break;
			}

			span = span[..split];

			if (permissions.TryGetValue(span, out value))
			{
				return true;
			}
		}

		return false;
	}

	public bool TryGetEntitlement(string entitlement, [NotNullWhen(true)] out string? value) => this.entitlements.TryGetValue(entitlement, out value);

	public ValueTask<bool> SetPermissionAsync(string permission, bool value) => ValueTask.FromResult(this.SetPermission(permission, value));

	internal bool SetPermission(string permission, bool value)
	{
		this.permissions[permission] = value;

		return true;
	}

	public ValueTask<bool> RemovePermissionAsync(string permission) => ValueTask.FromResult(this.permissions.TryRemove(permission, out _));

	public ValueTask<bool> SetEntitlementAsync(string entitlement, string value) => ValueTask.FromResult(this.SetEntitlement(entitlement, value));

	internal bool SetEntitlement(string entitlement, string value)
	{
		this.entitlements[entitlement] = value;

		return true;
	}

	public ValueTask<bool> RemoveEntitlementAsync(string entitlement) => ValueTask.FromResult(this.entitlements.TryRemove(entitlement, out _));

	public ValueTask<bool> AddParentAsync(IPermissionSubjectReference reference) => ValueTask.FromResult(this.AddParent(reference));
	public ValueTask<bool> RemoveParentAsync(IPermissionSubjectReference reference) => ValueTask.FromResult(this.RemoveParent(reference));

	internal bool AddParent(IPermissionSubjectReference reference)
	{
		this.parents[reference] = true;

		return true;
	}

	internal bool RemoveParent(IPermissionSubjectReference reference) => this.parents.TryRemove(reference, out _);
}
