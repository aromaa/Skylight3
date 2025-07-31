using Skylight.API.Game.Figure;
using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureSet(int id, IFigureSetType type, FigureSex? sex, IPermissionSubject? permissionRequirement, int colorLayers) : IFigureSet
{
	public int Id { get; } = id;

	public IFigureSetType Type { get; } = type;
	public FigureSex? Sex { get; } = sex;

	public int ColorLayers { get; } = colorLayers;

	private readonly IPermissionSubject? permissionRequirement = permissionRequirement;

	public bool CanWear(IPermissionSubject subject) => this.permissionRequirement is not { } permissionRequirement || subject.IsChildOf(permissionRequirement.Reference);
}
