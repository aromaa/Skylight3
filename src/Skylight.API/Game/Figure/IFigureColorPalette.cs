using System.Collections.Frozen;

namespace Skylight.API.Game.Figure;

public interface IFigureColorPalette
{
	public FrozenDictionary<int, IFigureColorPaletteColor> Colors { get; }
}
