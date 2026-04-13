using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal sealed class FigurePart(string key, IFigurePartType partType) : IFigurePart
{
	public string Key { get; } = key;
	public IFigurePartType PartType { get; } = partType;
}
