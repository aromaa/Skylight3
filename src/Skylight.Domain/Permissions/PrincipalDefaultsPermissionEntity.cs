namespace Skylight.Domain.Permissions;

public class PrincipalDefaultsPermissionEntity
{
	public string Identifier { get; set; } = null!;
	public string Permission { get; set; } = null!;

	public bool Value { get; set; }
}
