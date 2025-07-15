using Skylight.API.Game.Permissions;

namespace Skylight.Server.Extensions;

internal static class PermissionSubjectExtensions
{
	internal static int GetSecurityLevel(this IPermissionSubject subject) => !subject.TryGetEntitlement("skylight.user.security-level", out int value) ? 0 : value;
	internal static int GetClubLevel(this IPermissionSubject subject) => !subject.TryGetEntitlement("skylight.user.club-level", out int value) ? 0 : value;
}
