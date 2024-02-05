using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Drawing;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture.Wall;

internal sealed class StickyNoteFurniture(int id) : WallFurniture(id), IStickyNoteFurniture
{
	public FrozenSet<Color> ValidColors => ImmutableHashSet.Create(Color.FromArgb(0xFFFF33)).ToFrozenSet();

	public Color DefaultColor => Color.FromArgb(0xFFFF33);
}
