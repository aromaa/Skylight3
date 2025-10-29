namespace Skylight.Domain.Furniture;

public class FloorFurnitureEntity : FurnitureEntity
{
	public string Kind { get; set; } = null!;

	public int Width { get; set; }
	public int Length { get; set; }
	public List<double> Height { get; set; } = null!;
}
