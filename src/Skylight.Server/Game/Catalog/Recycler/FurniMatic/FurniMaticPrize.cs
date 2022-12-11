using System.Collections.Immutable;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Recycler.FurniMatic;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal sealed class FurniMaticPrize : IFurniMaticPrize
{
	public int Id { get; }
	public string Name { get; }

	public ImmutableArray<IFurniture> Furnitures { get; }

	internal FurniMaticPrize(int id, string name, ImmutableArray<IFurniture> furnitures)
	{
		this.Id = id;
		this.Name = name;

		this.Furnitures = furnitures;
	}
}
