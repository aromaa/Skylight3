using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureSetPart(IFigurePart part) : IFigureSetPart
{
	public IFigurePart Part { get; } = part;
}
