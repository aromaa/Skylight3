using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Badges;
using Skylight.Domain.Badges;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Badges;

internal sealed partial class BadgeManager : IBadgeManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private Snapshot snapshot;

	public BadgeManager(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;

		this.snapshot = new Snapshot(Cache.CreateBuilder().ToImmutable());
	}

	public IBadgeSnapshot Current => this.snapshot;

	public async Task<IBadgeSnapshot> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken)
	{
		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (BadgeEntity badge in dbContext.Badges
							   .AsNoTracking()
							   .AsAsyncEnumerable()
							   .WithCancellation(cancellationToken)
							   .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddBadge(badge);
			}
		}

		Snapshot snapshot = new(builder.ToImmutable());

		return context.Commit(() => this.snapshot = snapshot, snapshot);
	}
}
