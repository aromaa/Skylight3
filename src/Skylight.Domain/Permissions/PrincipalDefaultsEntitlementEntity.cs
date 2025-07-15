namespace Skylight.Domain.Permissions;

public class PrincipalDefaultsEntitlementEntity
{
	public string Identifier { get; set; } = null!;
	public string Entitlement { get; set; } = null!;

	public string Value { get; set; } = null!;
}
