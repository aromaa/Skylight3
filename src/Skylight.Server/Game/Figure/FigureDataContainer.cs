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

			foreach (IFigureColorPaletteColor color in value.Colors)
			{
				stringBuilder.Append('-');
				stringBuilder.Append(color.Id);
			}
		}

		return stringBuilder.ToString();
	}
}
