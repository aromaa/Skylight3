using System.Collections.Frozen;

namespace Skylight.API.Game.Figure;

public interface IFigureDataContainer
{
	public FrozenDictionary<IFigureSetType, FigureSetValue> Sets { get; }

	public string ToString();
}
