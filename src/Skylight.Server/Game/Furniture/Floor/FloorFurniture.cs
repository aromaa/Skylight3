using System.Collections.Immutable;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FloorFurniture : AbstractFurniture, IFloorFurniture
{
	internal int Width { get; }
	internal int Length { get; }
	public abstract double DefaultHeight { get; }

	public ImmutableArray<Point2D> EffectiveTiles { get; }

	internal FloorFurniture(int id, int width, int length)
		: base(id)
	{
		this.Width = width;
		this.Length = length;

		ImmutableArray<Point2D>.Builder effectiveTilesBuilder = ImmutableArray.CreateBuilder<Point2D>(width * length);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < length; j++)
			{
				effectiveTilesBuilder.Add(new Point2D(i, j));
			}
		}

		this.EffectiveTiles = effectiveTilesBuilder.ToImmutable();
	}

	public EffectiveTilesEnumerator GetEffectiveTiles(int direction) => new(this.EffectiveTiles, direction);
}
