using System.Collections.Immutable;

namespace Skylight.API.Game.Furniture.Floor;

public interface IVariableHeightFurniture : IMultiStateFloorFurniture, IInteractableFurniture
{
	public ImmutableArray<double> Heights { get; }
}
