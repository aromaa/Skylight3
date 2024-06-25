using System.Collections.Immutable;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class VariableHeightFurniture(int id, FloorFurnitureType type, Point2D dimensions, ImmutableArray<double> heights, int stateCount) : MultiStateFloorFurniture(id, type, dimensions, stateCount), IVariableHeightFurniture
{
	public ImmutableArray<double> Heights { get; } = heights;

	public override double DefaultHeight => this.Heights[0];
}
