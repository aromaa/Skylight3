using Microsoft.EntityFrameworkCore;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;
using Skylight.Domain.Achievements;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Achievements;

internal sealed partial class AchievementManager : IAchievementManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IBadgeManager badgeManager;

	private Snapshot snapshot;

	public AchievementManager(IDbContextFactory<SkylightContext> dbContextFactory, IBadgeManager badgeManager)
	{
		this.dbContextFactory = dbContextFactory;

		this.badgeManager = badgeManager;

		this.snapshot = new Snapshot(Cache.CreateBuilder().ToImmutable(badgeManager.Current));
	}

	public IAchievementSnapshot Current => this.snapshot;

	public async Task<IAchievementSnapshot> LoadAsync(CancellationToken cancellationToken = default)
	{
		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (AchievementEntity entity in dbContext.Achievements
						 .AsNoTrackingWithIdentityResolution()
						 .AsSplitQuery()
						 .Include(a => a.Levels!.OrderBy(l => l.Level))
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken)
						 .ConfigureAwait(false))
			{
				builder.AddAchievement(entity);
			}
		}

		return this.snapshot = new Snapshot(builder.ToImmutable(this.badgeManager.Current));
	}
}
