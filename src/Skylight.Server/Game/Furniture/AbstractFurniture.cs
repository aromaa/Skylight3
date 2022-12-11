using Skylight.API.Game.Furniture;

namespace Skylight.Server.Game.Furniture;

internal abstract class AbstractFurniture : IFurniture
{
	public int Id { get; }

	protected AbstractFurniture(int id)
	{
		this.Id = id;
	}
}
