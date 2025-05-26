using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Furniture;
using Skylight.API.Registry;
using Skylight.Domain.Furniture;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Furniture;

internal sealed partial class FurnitureManager(IDbContextFactory<SkylightContext> dbContextFactory, IRegistryHolder registryHolder) : LoadableServiceBase<IFurnitureSnapshot>(new Snapshot(Cache.CreateBuilder().ToImmutable(registryHolder))), IFurnitureManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IRegistryHolder registryHolder = registryHolder;

	public override async Task<IFurnitureSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default)
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

		return new Snapshot(builder.ToImmutable(this.registryHolder));
	}
}
