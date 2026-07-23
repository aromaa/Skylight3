using System.Collections.Frozen;
using System.Text;
using Skylight.API.Game.Figure;

namespace Skylight.Server.Game.Figure;

internal sealed class FigureDataContainer(FrozenDictionary<IFigureSetType, FigureSetValue> sets) : IFigureDataContainer
{
	public FrozenDictionary<IFigureSetType, FigureSetValue> Sets { get; } = sets;

	public override string ToString()
	{
		StringBuilder stringBuilder = new();
		foreach ((IFigureSetType setType, FigureSetValue value) in this.Sets)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('.');
			}

			stringBuilder.Append(setType.Type);
			stringBuilder.Append('-');
			stringBuilder.Append(value.Set.Id);

			if (value.Colors.Length > 0)
			{
				foreach (IFigureColorPaletteColor color in value.Colors)
				{
					stringBuilder.Append('-');
					stringBuilder.Append(color.Id);
				}
			}
			else
			{
				// Shockwave client requires at least three parts,
				// whatever it will actually use the color or not.
				// Just send a dash to fill this requirement.
				stringBuilder.Append('-');
			}
		}

		return stringBuilder.ToString();
	}
}
