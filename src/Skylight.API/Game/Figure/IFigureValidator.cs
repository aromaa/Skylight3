using Skylight.API.Game.Permissions;

namespace Skylight.API.Game.Figure;

public interface IFigureValidator
{
	public bool Validate(IFigureSet set, IPermissionSubject? subject = null);

	// TODO: Better API design
	public HashSet<IFigureSetType> Validate(IFigureDataContainer container, IPermissionSubject? subject = null);
}
