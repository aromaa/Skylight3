using System.Collections.Frozen;
using System.Drawing;

namespace Skylight.API.Game.Furniture.Wall;

public interface IStickyNoteFurniture : IComplexWallFurniture
{
	public FrozenSet<Color> ValidColors { get; }

	public Color DefaultColor { get; }
}
