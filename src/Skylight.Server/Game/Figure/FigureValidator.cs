using Skylight.API.Game.Figure;
using Skylight.API.Game.Permissions;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureValidator(Dictionary<IFigureSetType, HashSet<IPermissionSubject>> figureSetTypeRules) : IFigureValidator
{
	private readonly Dictionary<IFigureSetType, HashSet<IPermissionSubject>> figureSetTypeRules = figureSetTypeRules;

	public bool Validate(IFigureSet set, IPermissionSubject? subject = null) => true;

	public HashSet<IFigureSetType> Validate(IFigureDataContainer container, IPermissionSubject? subject = null)
	{
		HashSet<IFigureSetType>? invalidTypes = null;
		foreach ((IFigureSetType setType, HashSet<IPermissionSubject> exemptRanks) in this.figureSetTypeRules)
		{
			if (container.Sets.ContainsKey(setType))
			{
				continue;
			}

			if (subject is not null)
			{
				if (exemptRanks.Count == 0)
				{
					invalidTypes ??= [];
					invalidTypes.Add(setType);

					continue;
				}

				foreach (IPermissionSubject permissionSubject in exemptRanks)
				{
					if (subject.IsChildOf(permissionSubject.Reference))
					{
						continue;
					}

					invalidTypes ??= [];
					invalidTypes.Add(setType);

					break;
				}
			}
			else
			{
				invalidTypes ??= [];
				invalidTypes.Add(setType);
			}
		}

		return invalidTypes ?? [];
	}
}
