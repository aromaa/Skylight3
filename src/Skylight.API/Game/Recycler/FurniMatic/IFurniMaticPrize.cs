using System.Collections.Immutable;
using Skylight.API.Game.Furniture;

namespace Skylight.API.Game.Recycler.FurniMatic;

public interface IFurniMaticPrize
{
	public int Id { get; }
	public string Name { get; }

	public ImmutableArray<IFurniture> Furnitures { get; }
}
