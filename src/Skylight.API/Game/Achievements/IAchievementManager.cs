using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Achievements;

public interface IAchievementManager : IAchievementSnapshot, ILoadableService<IAchievementSnapshot>
{
}
