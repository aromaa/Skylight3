using System.Collections.Immutable;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal abstract class FloorFurniture : AbstractFurniture, IFloorFurniture
{
	public IFloorFurnitureKind Kind { get; }

	public Point2D Dimensions { get; }
	public ImmutableArray<Point2D> EffectiveTiles { get; }

	public abstract double DefaultHeight { get; }

	internal FloorFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions)
		: base(id)
	{
		this.Kind = kind;
		this.Dimensions = dimensions;

		ImmutableArray<Point2D>.Builder effectiveTilesBuilder = ImmutableArray.CreateBuilder<Point2D>(dimensions.X * dimensions.Y);
		for (int i = 0; i < dimensions.X; i++)
		{
			for (int j = 0; j < dimensions.Y; j++)
			{
				effectiveTilesBuilder.Add(new Point2D(i, j));
			}
		}

		this.EffectiveTiles = effectiveTilesBuilder.ToImmutable();
	}

	public EffectiveTilesEnumerator GetEffectiveTiles(int direction) => new(this.EffectiveTiles, direction);
}
