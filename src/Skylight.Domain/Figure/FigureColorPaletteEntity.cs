namespace Skylight.Domain.Figure;

public class FigureColorPaletteEntity
{
	public int Id { get; init; }

	public List<FigureColorPaletteColorEntity>? Colors { get; set; }
}
