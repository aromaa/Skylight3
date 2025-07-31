using System.Collections.Frozen;
using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureColorPalette(FrozenDictionary<int, IFigureColorPaletteColor> colors) : IFigureColorPalette
{
	public FrozenDictionary<int, IFigureColorPaletteColor> Colors { get; } = colors;
}
