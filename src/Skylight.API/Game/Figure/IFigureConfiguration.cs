using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Game.Figure;

public interface IFigureConfiguration
{
	public IFigureDataContainer Parse(ReadOnlySequence<byte> figure, FigureValidationOptions validationOptions = default);
	public IFigureDataContainer Parse(ReadOnlySpan<char> figure, FigureValidationOptions validationOptions = default);

	public bool TryGetColorPaletteColor(int id, [NotNullWhen(true)] out IFigureColorPaletteColor? colorPaletteColor);
	public bool TryGetFigureSetType(int id, [NotNullWhen(true)] out IFigureSetType? figureSetType);
	public bool TryGetFigureSet(int id, [NotNullWhen(true)] out IFigureSet? figureSet);

	public bool TryGetFigureValidator(string name, FigureSex sex, [NotNullWhen(true)] out IFigureValidator? validator);
}
