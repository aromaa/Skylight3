namespace Skylight.Domain.Figure;

public class FigureSetTypeEntity
{
	public int Id { get; init; }

	public string Type { get; set; } = null!;

	public int ColorPaletteId { get; set; }
	public FigureColorPaletteEntity? ColorPalette { get; set; }

	public List<FigureSetEntity>? Sets { get; set; }
}
