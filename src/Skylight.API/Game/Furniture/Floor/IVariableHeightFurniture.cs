using System.Collections.Immutable;

namespace Skylight.API.Game.Furniture.Floor;

public interface IVariableHeightFurniture : IFloorFurniture, IMultiStateFurniture
{
	public ImmutableArray<double> Heights { get; }
}
