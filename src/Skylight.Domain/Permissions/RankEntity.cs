namespace Skylight.Domain.Permissions;

public class RankEntity
{
	public string Id { get; init; } = null!;

	public List<RankPermissionEntity>? Permissions { get; set; }
	public List<RankEntitlementEntity>? Entitlements { get; set; }
	public List<RankChildEntity>? Children { get; set; }
}
