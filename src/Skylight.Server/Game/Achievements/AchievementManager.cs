using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;
using Skylight.Domain.Achievements;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Achievements;

internal sealed partial class AchievementManager : IAchievementManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private Snapshot snapshot;

	public AchievementManager(IDbContextFactory<SkylightContext> dbContextFactory, IBadgeManager badgeManager)
	{
		this.dbContextFactory = dbContextFactory;

		this.snapshot = new Snapshot(Cache.CreateBuilder().ToImmutable(badgeManager));
	}

	public IAchievementSnapshot Current => this.snapshot;

	public async Task<IAchievementSnapshot> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		Task<IBadgeSnapshot> badgeSnapshot = context.RequestDependencyAsync<IBadgeSnapshot>(cancellationToken);

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
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddAchievement(entity);
			}
		}

		Snapshot snapshot = new(builder.ToImmutable(await badgeSnapshot.ConfigureAwait(false)));

		return context.Commit(() => this.snapshot = snapshot, snapshot);
	}
}
