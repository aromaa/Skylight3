using Skylight.Domain.Permissions;

namespace Skylight.Domain.Figure;

public class FigureColorPaletteColorEntity
{
	public int Id { get; init; }

	public int PaletteId { get; set; }
	public FigureColorPaletteEntity? Palette { get; set; }

	public string? RankId { get; set; }
	public RankEntity? Rank { get; set; }

	public int OrderNum { get; set; }

	public bool Selectable { get; set; }

	public int Color { get; set; }
}
