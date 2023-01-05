namespace Skylight.API.Game.Achievements;

public interface IAchievementManager : IAchievementSnapshot
{
	public IAchievementSnapshot Current { get; }

	public Task<IAchievementSnapshot> LoadAsync(CancellationToken cancellationToken = default);
}
