using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Badges;

public interface IBadgeManager : IBadgeSnapshot, ILoadableService<IBadgeSnapshot>
{
}
