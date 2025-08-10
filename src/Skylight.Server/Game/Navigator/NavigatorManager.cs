using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Skylight.API.Collections.Cache;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Domain.Navigator;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Private;
using Skylight.Domain.Rooms.Public;
using Skylight.Infrastructure;
using Skylight.Server.Collections.Cache;
using Skylight.Server.DependencyInjection;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Navigator;

internal sealed partial class NavigatorManager : VersionedLoadableServiceBase<INavigatorSnapshot, NavigatorSnapshot>, INavigatorManager
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly IUserManager userManager;

	private readonly AsyncCache<int, IPrivateRoomInfo> roomData;

	public NavigatorManager(IDbContextFactory<SkylightContext> dbContextFactory, IUserManager userManager)
		: base(NavigatorSnapshot.CreateBuilder().Build())
	{
		this.dbContextFactory = dbContextFactory;

		this.userManager = userManager;

		this.roomData = new AsyncCache<int, IPrivateRoomInfo>(this.InternalLoadRoomDataAsync);
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

	public ValueTask<IPrivateRoomInfo?> GetPrivateRoomInfoAsync(int id, CancellationToken cancellationToken)
	{
		return this.roomData.GetAsync(id);
	}

	public ValueTask<ICacheReference<IPrivateRoomInfo>?> GetPrivateRoomInfoUnsafeAsync(int roomId, CancellationToken cancellationToken = default)
	{
		return this.roomData.GetValueAsync(roomId)!;
	}

	private async Task<IPrivateRoomInfo?> InternalLoadRoomDataAsync(int id)
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

		if (!this.TryGetNode(entity.CategoryId, out IServiceValue<INavigatorCategoryNode>? category))
		{
			throw new InvalidOperationException($"Missing category {entity.CategoryId}");
		}

		IUserInfo? owner = await this.userManager.GetUserInfoAsync(entity.OwnerId).ConfigureAwait(false);
		if (owner is null)
		{
			throw new InvalidOperationException($"Missing owner {entity.OwnerId}");
		}

		RoomEntryMode entryMode = entity.EntryMode switch
		{
			PrivateRoomEntryMode.Open => RoomEntryMode.Open(),
			PrivateRoomEntryMode.Locked => RoomEntryMode.Locked(),
			PrivateRoomEntryMode.Password => RoomEntryMode.Password(entity.Password!),
			PrivateRoomEntryMode.Invisible => RoomEntryMode.Invisible(),
			PrivateRoomEntryMode.NoobsOnly => RoomEntryMode.NoobsOnly(),

			_ => throw new InvalidOperationException($"Unknown entry mode: {entity.EntryMode}")
		};

		RoomTradeMode tradeMode = entity.TradeMode switch
		{
			PrivateRoomTradeMode.None => RoomTradeMode.None,
			PrivateRoomTradeMode.WithRights => RoomTradeMode.WithRights,
			PrivateRoomTradeMode.Everyone => RoomTradeMode.Everyone,

			_ => throw new InvalidOperationException($"Unknown trade mode: {entity.TradeMode}")
		};

		return new PrivateRoomInfo(id, owner, layout, new PrivateRoomSettings(entity.Name, entity.Description, category, ImmutableCollectionsMarshal.AsImmutableArray(entity.Tags), entryMode, entity.UsersMax, tradeMode, entity.WalkThrough, entity.AllowPets, entity.AllowPetsToEat, new PrivateRoomCustomizationSettings(entity.HideWalls, entity.FloorThickness, entity.WallThickness), new PrivateRoomChatSettings(), new PrivateRoomModerationSettings()));
	}
}
