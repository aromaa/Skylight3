namespace Skylight.API.Game.Figure;

public interface IFigurePart
{
	public IFigurePartType PartType { get; }

	public string Key { get; }
}
