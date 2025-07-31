using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Permissions;
using Skylight.Domain.Figure;
using Skylight.Infrastructure;
using Skylight.Server.DependencyInjection;

namespace Skylight.Server.Game.Figure;

internal sealed partial class FigureConfigurationManager(IDbContextFactory<SkylightContext> dbContextFactory) : LoadableServiceBase<IFigureConfigurationSnapshot>(new Snapshot(Cache.CreateBuilder().ToImmutable())), IFigureConfigurationManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	public override async Task<IFigureConfigurationSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		Task<IPermissionManager> permissionManager = context.RequestServiceAsync<IPermissionManager>(cancellationToken);

		Cache.Builder builder = Cache.CreateBuilder();
		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (FigureColorPaletteEntity paletteEntity in dbContext.FigurePalettes
				.AsSplitQuery()
				.Include(p => p.Colors)
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				builder.AddPalette(paletteEntity);
			}

			await foreach (FigureSetTypeEntity figureSetTypeEntity in dbContext.FigureSetTypes
				.AsSplitQuery()
				.Include(s => s.Sets!)
				.ThenInclude(s => s.Parts)
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				builder.AddFigureTypeSet(figureSetTypeEntity);
			}

			await foreach (FigureValidationEntity figureValidationEntity in dbContext.FigureValidations
				.AsSplitQuery()
				.Include(e => e.SetTypeRules!)
				.ThenInclude(e => e.ExemptRanks)
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				builder.AddFigureValidation(figureValidationEntity);
			}
		}

		return new Snapshot(await builder.ToImmutableAsync(await permissionManager.ConfigureAwait(false), cancellationToken).ConfigureAwait(false));
	}
}
