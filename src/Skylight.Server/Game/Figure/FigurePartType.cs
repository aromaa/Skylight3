using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal sealed class FigurePartType(string type) : IFigurePartType
{
	public string Type { get; } = type;
}
