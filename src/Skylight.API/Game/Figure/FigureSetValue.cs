using System.Collections.Immutable;

namespace Skylight.API.Game.Figure;

public record struct FigureSetValue(IFigureSet Set, ImmutableArray<IFigureColorPaletteColor> Colors);
