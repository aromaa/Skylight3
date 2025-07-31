using Skylight.API.Game.Figure;
using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureColorPaletteColor(int id, IPermissionSubject? permissionRequirement) : IFigureColorPaletteColor
{
	public int Id { get; } = id;

	private readonly IPermissionSubject? permissionRequirement = permissionRequirement;

	public bool CanWear(IPermissionSubject subject) => this.permissionRequirement is not { } permissionRequirement || subject.IsChildOf(permissionRequirement.Reference);
}
