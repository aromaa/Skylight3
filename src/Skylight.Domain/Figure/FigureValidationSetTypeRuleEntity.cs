using Skylight.Domain.Permissions;

namespace Skylight.Domain.Figure;

public class FigureValidationSetTypeRuleEntity
{
	public int Id { get; init; }

	public int ValidationId { get; set; }
	public FigureValidationEntity? Validation { get; set; }

	public int SetTypeId { get; set; }
	public FigureSetTypeEntity? SetType { get; set; }

	public List<RankEntity>? ExemptRanks { get; set; }
}
