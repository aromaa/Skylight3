using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Recycler.FurniMatic;

public interface IFurniMaticManager : IFurniMaticSnapshot, ILoadableService<IFurniMaticSnapshot>
{
}
