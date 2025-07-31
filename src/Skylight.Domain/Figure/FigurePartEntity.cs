namespace Skylight.Domain.Figure;

public class FigurePartEntity
{
	public int Id { get; init; }

	public int PartTypeId { get; set; }
	public FigurePartTypeEntity? PartType { get; set; }

	public string Key { get; set; } = null!;
}
