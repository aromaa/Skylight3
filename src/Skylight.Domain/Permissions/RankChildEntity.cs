namespace Skylight.Domain.Permissions;

public class RankChildEntity
{
	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }

	public string ChildRankId { get; set; } = null!;
	public RankEntity? ChildRank { get; set; }
}
