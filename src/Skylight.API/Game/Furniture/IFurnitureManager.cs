using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureManager : IFurnitureSnapshot, ILoadableService<IFurnitureSnapshot>
{
}
