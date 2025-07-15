namespace Skylight.Domain.Permissions;

public class PrincipalDefaultsRankEntity
{
	public string Identifier { get; set; } = null!;

	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }
}
