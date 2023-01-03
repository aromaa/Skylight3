namespace Skylight.API.Game.Badges;

public interface IBadgeManager : IBadgeSnapshot
{
	public IBadgeSnapshot Current { get; }

	public Task<IBadgeSnapshot> LoadAsync(CancellationToken cancellationToken = default);
}
