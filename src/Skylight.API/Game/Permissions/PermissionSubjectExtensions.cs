using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Skylight.API.Game.Permissions;

public static class PermissionSubjectExtensions
{
	public static bool TryGetEntitlement<T>(this IPermissionSubject subject, string entitlement, [MaybeNullWhen(false)] out T value)
		where T : IParsable<T>
	{
		if (subject.TryGetEntitlement(entitlement, out string? stringValue) && T.TryParse(stringValue, CultureInfo.InvariantCulture, out value))
		{
			return true;
		}

		value = default;
		return false;
	}
}
