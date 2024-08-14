using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Furniture;

public interface IFurnitureManager : IFurnitures, ILoadableService<IFurnitureSnapshot>;
