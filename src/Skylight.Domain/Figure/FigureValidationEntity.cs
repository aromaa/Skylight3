namespace Skylight.Domain.Figure;

public class FigureValidationEntity
{
	public int Id { get; init; }

	public string Name { get; set; } = null!;
	public FigureSexType Sex { get; set; }

	public List<FigureValidationSetTypeRuleEntity>? SetTypeRules { get; set; }
}
