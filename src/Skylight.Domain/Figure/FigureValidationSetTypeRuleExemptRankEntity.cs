using Skylight.Domain.Permissions;

namespace Skylight.Domain.Figure;

public class FigureValidationSetTypeRuleExemptRankEntity
{
	public int SetTypeRuleId { get; set; }
	public FigureValidationSetTypeRuleEntity? SetTypeRule { get; set; }

	public string RankId { get; set; } = null!;
	public RankEntity? RankEntity { get; set; }
}
