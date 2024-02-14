using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms;
using Skylight.Domain.Rooms.Layout;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Rooms;

namespace Skylight.Server.Game.Navigator;

internal sealed partial class NavigatorManager : LoadableServiceBase<INavigatorSnapshot>, INavigatorManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IUserManager userManager;

	private readonly AsyncTypedCache<int, IRoomInfo?> roomData;

	public NavigatorManager(IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager)
		: base(new Snapshot(Cache.CreateBuilder().ToImmutable()))
	{
		this.dbContextFactory = dbContextFactory;

		this.userManager = userManager;

		this.roomData = new AsyncTypedCache<int, IRoomInfo?>(this.InternalLoadRoomDataAsync);
	}

	public override async Task<INavigatorSnapshot> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken)
	{
		Cache.Builder builder = Cache.CreateBuilder();

		await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
		{
			await foreach (RoomLayoutEntity layout in dbContext.RoomLayouts
						 .AsNoTracking()
						 .AsAsyncEnumerable()
						 .WithCancellation(cancellationToken)
						 .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddLayout(layout);
			}

			await foreach (RoomFlatCatEntity flatCat in dbContext.FlatCats
							  .AsNoTracking()
							  .AsAsyncEnumerable()
							  .WithCancellation(cancellationToken)
							  .ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddFlatCat(flatCat);
			}
		}

		return new Snapshot(builder.ToImmutable());
	}

	public ValueTask<IRoomInfo?> GetRoomDataAsync(int id, CancellationToken cancellationToken)
	{
		return this.roomData.GetAsync(id);
	}

	private async Task<IRoomInfo?> InternalLoadRoomDataAsync(int id)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

		RoomEntity? entity = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == id).ConfigureAwait(false);
		if (entity is null)
		{
			return null;
		}

		if (!this.TryGetLayout(entity.LayoutId, out IRoomLayout? layout))
		{
			throw new InvalidOperationException($"Missing room layout data for {entity.LayoutId}");
		}

		IUserInfo? owner = await this.userManager.GetUserInfoAsync(entity.OwnerId).ConfigureAwait(false);

		return new RoomData(entity, owner!, layout);
	}
}
