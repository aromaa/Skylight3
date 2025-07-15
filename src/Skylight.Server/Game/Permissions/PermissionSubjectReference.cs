using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Permissions;

internal sealed class PermissionSubjectReference<T>(PermissionManager permissionManager, string directory, T identifier) : IPermissionSubjectReference<T>, IEquatable<PermissionSubjectReference<T>>
{
	private readonly PermissionManager permissionManager = permissionManager;

	public string Directory { get; } = directory;
	public T Identifier { get; } = identifier;

	public async ValueTask<IPermissionSubject?> Resolve()
	{
		IPermissionDirectory? directory = await this.permissionManager.GetDirectoryAsync(this.Directory).ConfigureAwait(false);
		if (directory is IPermissionDirectory<T> typedDirectory)
		{
			return await typedDirectory.GetSubjectAsync(this.Identifier).ConfigureAwait(false);
		}

		return null;
	}

	public bool Equals(PermissionSubjectReference<T>? other)
	{
		if (other is null)
		{
			return false;
		}

		if (object.ReferenceEquals(this, other))
		{
			return true;
		}

		return this.permissionManager == other.permissionManager && this.Directory == other.Directory && EqualityComparer<T>.Default.Equals(this.Identifier, other.Identifier);
	}

	public override bool Equals(object? obj) => obj is PermissionSubjectReference<T> other && this.Equals(other);
	public override int GetHashCode() => HashCode.Combine(this.permissionManager, this.Directory, this.Identifier);

	public static bool operator ==(PermissionSubjectReference<T>? left, PermissionSubjectReference<T>? right) => object.Equals(left, right);
	public static bool operator !=(PermissionSubjectReference<T>? left, PermissionSubjectReference<T>? right) => !object.Equals(left, right);
}
