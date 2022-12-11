using System.Collections.Immutable;
using System.Drawing;

namespace Skylight.API.Game.Furniture.Wall;

public interface IStickyNoteFurniture : IWallFurniture
{
	public ImmutableHashSet<Color> ValidColors { get; }

	public Color DefaultColor { get; }
}
