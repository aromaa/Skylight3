using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.Domain.Furniture;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Furniture;

internal sealed partial class FurnitureManager : IFurnitureManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private Snapshot snapshot;

	public FurnitureManager(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;

		this.snapshot = new Snapshot(Cache.CreateBuilder().ToImmutable());
	}

	public IFurnitureSnapshot Current => this.snapshot;

	public async Task<IFurnitureSnapshot> LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken)
	{
		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (FloorFurnitureEntity furniture in dbContext.FloorFurniture
							   .AsNoTracking()
							   .AsAsyncEnumerable()
							   .WithCancellation(cancellationToken)
							   .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddFloorItem(furniture);
			}

			await foreach (WallFurnitureEntity furniture in dbContext.WallFurniture
							   .AsNoTracking()
							   .AsAsyncEnumerable()
							   .WithCancellation(cancellationToken)
							   .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddWallItem(furniture);
			}
		}

		Snapshot snapshot = new(builder.ToImmutable());

		return context.Commit(() => this.snapshot = snapshot, snapshot);
	}
}
