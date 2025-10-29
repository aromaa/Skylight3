namespace Skylight.Domain.Permissions;

public class AccessSetRankRuleEntity : AccessSetRuleEntity
{
	public string Partition { get; set; } = null!;
	public OperationType Operation { get; set; }

	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }

	public enum OperationType
	{
		And,
		Or
	}
}
