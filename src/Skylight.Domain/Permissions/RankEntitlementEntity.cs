namespace Skylight.Domain.Permissions;

public class RankEntitlementEntity
{
	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }

	public string Entitlement { get; set; } = null!;
	public string Value { get; set; } = null!;
}
