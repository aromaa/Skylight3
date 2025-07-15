namespace Skylight.Domain.Permissions;

public class RankPermissionEntity
{
	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }

	public string Permission { get; set; } = null!;
	public bool Value { get; set; }
}
