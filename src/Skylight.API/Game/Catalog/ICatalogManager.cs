using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Catalog;

public interface ICatalogManager : ICatalogSnapshot, ILoadableService<ICatalogSnapshot>
{
}
