using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Permissions;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionSubject(IPermissionDirectory directory) : IPermissionSubject
{
	public IPermissionDirectory Directory { get; } = directory;

	internal PermissionContainer Container { get; } = new();
	internal PermissionContainer TransientContainer { get; } = new();

	public bool TryGetPermission(string permission, out bool value)
	{
		if (this.TransientContainer.TryGetPermission(permission, out value)
			|| this.Container.TryGetPermission(permission, out value))
		{
			return true;
		}

		foreach (IPermissionSubjectReference parentReference in this.TransientContainer.Parents)
		{
			IPermissionSubject? parent = parentReference.Resolve().Wait();
			if (parent is not null && parent.TryGetPermission(permission, out value))
			{
				return true;
			}
		}

		foreach (IPermissionSubjectReference parentReference in this.Container.Parents)
		{
			IPermissionSubject? parent = parentReference.Resolve().Wait();
			if (parent is not null && parent.TryGetPermission(permission, out value))
			{
				return true;
			}
		}

		IPermissionSubject defaults = this.Directory.Defaults;

		return defaults != this && defaults.TryGetPermission(permission, out value);
	}

	public bool TryGetEntitlement(string entitlement, [NotNullWhen(true)] out string? value)
	{
		if (this.TransientContainer.TryGetEntitlement(entitlement, out value)
			|| this.Container.TryGetEntitlement(entitlement, out value))
		{
			return true;
		}

		foreach (IPermissionSubjectReference parentReference in this.TransientContainer.Parents)
		{
			IPermissionSubject? parent = parentReference.Resolve().Wait();
			if (parent is not null && parent.TryGetEntitlement(entitlement, out value))
			{
				return true;
			}
		}

		foreach (IPermissionSubjectReference parentReference in this.Container.Parents)
		{
			IPermissionSubject? parent = parentReference.Resolve().Wait();
			if (parent is not null && parent.TryGetEntitlement(entitlement, out value))
			{
				return true;
			}
		}

		IPermissionSubject defaults = this.Directory.Defaults;

		return defaults.Directory != this.Directory && defaults.TryGetEntitlement(entitlement, out value);
	}

	IPermissionContainer IPermissionSubject.Container => this.Container;
	IPermissionContainer IPermissionSubject.TransientContainer => this.TransientContainer;
}
