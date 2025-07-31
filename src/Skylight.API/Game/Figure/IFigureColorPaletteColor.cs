using Skylight.API.Game.Permissions;

namespace Skylight.API.Game.Figure;

public interface IFigureColorPaletteColor
{
	public int Id { get; }

	public bool CanWear(IPermissionSubject subject);
}
