using System.Collections.Immutable;
using System.Drawing;
using Skylight.API.Game.Furniture.Wall;

namespace Skylight.Server.Game.Furniture.Wall;

internal sealed class StickyNoteFurniture : WallFurniture, IStickyNoteFurniture
{
	public ImmutableHashSet<Color> ValidColors => ImmutableHashSet.Create(Color.FromArgb(0xFFFF33));

	public Color DefaultColor => Color.FromArgb(0xFFFF33);

	internal StickyNoteFurniture(int id)
		: base(id)
	{
	}
}
