namespace Skylight.Domain.Figure;

public class FigureSetPartEntity
{
	public int SetId { get; set; }
	public FigureSetEntity? Set { get; set; }

	public int PartId { get; set; }
	public FigurePartEntity? Part { get; set; }

	public int OrderNum { get; set; }
	public int ColorIndex { get; set; }
}
