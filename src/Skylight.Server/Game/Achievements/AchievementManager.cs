using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Badges;
using Skylight.Domain.Achievements;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Achievements;

internal sealed partial class AchievementManager(IDbContextFactory<SkylightContext> dbContextFactory, IBadgeManager badgeManager)
	: LoadableServiceBase<IAchievementSnapshot>(new Snapshot(Cache.CreateBuilder().ToImmutable(badgeManager.Current))), IAchievementManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	public override async Task<IAchievementSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default)
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

		return new Snapshot(builder.ToImmutable(await badgeSnapshot.ConfigureAwait(false)));
	}
}
