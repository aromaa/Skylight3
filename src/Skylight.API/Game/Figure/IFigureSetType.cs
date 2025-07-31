using System.Collections.Frozen;

namespace Skylight.API.Game.Figure;

public interface IFigureSetType
{
	public int Id { get; }
	public string Type { get; }

	public IFigureColorPalette ColorPalette { get; }

	public FrozenDictionary<int, IFigureSet> Sets { get; }
}
