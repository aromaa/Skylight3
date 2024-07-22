using Microsoft.EntityFrameworkCore;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Private;
using Skylight.Domain.Rooms.Public;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Rooms;

namespace Skylight.Server.Game.Navigator;

internal sealed partial class NavigatorManager : VersionedLoadableServiceBase<INavigatorSnapshot, NavigatorSnapshot>, INavigatorManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IUserManager userManager;

	private readonly AsyncTypedCache<int, IRoomInfo?> roomData;

	public NavigatorManager(IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager)
		: base(NavigatorSnapshot.CreateBuilder().Build())
	{
		this.dbContextFactory = dbContextFactory;

		this.userManager = userManager;

		this.roomData = new AsyncTypedCache<int, IRoomInfo?>(this.InternalLoadRoomDataAsync);
	}

	internal override async Task<VersionedServiceSnapshot.Transaction<NavigatorSnapshot>> LoadAsyncCore(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		NavigatorSnapshot.Builder builder = NavigatorSnapshot.CreateBuilder();

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

			await foreach (PublicRoomEntity publicRoom in dbContext.PublicRooms
				.AsNoTracking()
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddPublicRoom(publicRoom);
			}

			await foreach (NavigatorNodeEntity node in dbContext.NavigatorNodes
				.Include(e => e.Children)
				.AsNoTrackingWithIdentityResolution()
				.AsAsyncEnumerable()
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false))
			{
				cancellationToken.ThrowIfCancellationRequested();

				builder.AddFlatCat(node);
			}
		}

		return builder.BuildAndStartTransaction(this, this.Current);
	}

	public ValueTask<IRoomInfo?> GetRoomDataAsync(int id, CancellationToken cancellationToken)
	{
		return this.roomData.GetAsync(id);
	}

	private async Task<IRoomInfo?> InternalLoadRoomDataAsync(int id)
	{
		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

		PrivateRoomEntity? entity = await dbContext.PrivateRooms.FirstOrDefaultAsync(r => r.Id == id).ConfigureAwait(false);
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
