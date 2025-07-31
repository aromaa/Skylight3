using System.Collections.Frozen;
using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureSetType(int id, string type, IFigureColorPalette colorPalette) : IFigureSetType
{
	public int Id { get; } = id;
	public string Type { get; } = type;

	public IFigureColorPalette ColorPalette { get; } = colorPalette;

	public FrozenDictionary<int, IFigureSet> Sets { get; set; } = null!;

	internal void Init(FrozenDictionary<int, IFigureSet> sets)
	{
		this.Sets = sets;
	}
}
