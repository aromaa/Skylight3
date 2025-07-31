using Skylight.API.Game.Permissions;

namespace Skylight.API.Game.Figure;

public interface IFigureSet
{
	public int Id { get; }

	public IFigureSetType Type { get; }
	public FigureSex? Sex { get; }

	public bool CanWear(IPermissionSubject subject);

	public int ColorLayers { get; }
}
