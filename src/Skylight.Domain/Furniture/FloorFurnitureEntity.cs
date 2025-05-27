namespace Skylight.Domain.Furniture;

public class FloorFurnitureEntity
{
	public int Id { get; init; }

	public string Kind { get; set; } = null!;

	public string ClassName { get; init; } = null!;

	public int Width { get; set; }
	public int Length { get; set; }
	public List<double> Height { get; set; } = null!;

	public string InteractionType { get; set; } = null!;
	public string InteractionData { get; set; } = null!;
}
