using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Badges;
using Skylight.Domain.Badges;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Badges;

internal sealed partial class BadgeManager : LoadableServiceBase<IBadgeSnapshot>, IBadgeManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public BadgeManager(IDbContextFactory<SkylightContext> dbContextFactory)
		: base(new Snapshot(Cache.CreateBuilder().ToImmutable()))
	{
		this.dbContextFactory = dbContextFactory;
	}

	public override async Task<IBadgeSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken)
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

		return new Snapshot(builder.ToImmutable());
	}
}
