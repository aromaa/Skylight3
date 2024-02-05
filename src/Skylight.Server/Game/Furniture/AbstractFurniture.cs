using Skylight.API.Game.Furniture;

namespace Skylight.Server.Game.Furniture;

internal abstract class AbstractFurniture(int id) : IFurniture
{
	public int Id { get; } = id;
}
