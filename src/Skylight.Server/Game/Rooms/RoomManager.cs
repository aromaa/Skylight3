using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Skylight.API.Collections.Cache;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Registry;
using Skylight.Infrastructure;
using Skylight.Server.Game.Rooms.Private;
using Skylight.Server.Game.Rooms.Public;

namespace Skylight.Server.Game.Rooms;

internal sealed partial class RoomManager : IRoomManager, ILoadableService
{
	private readonly RoomSettings roomSettings;

	private readonly ConcurrentDictionary<int, ICacheReference<IPrivateRoom>> forceLoadedRooms = [];

	private readonly Dictionary<IRoomType, IRoomTypeManager> roomTypeManagers = [];

	private readonly IRoomType<IPrivateRoom, IPrivateRoomInfo, int> privateRoomType;

	public RoomManager(IServiceProvider serviceProvider, IRegistryHolder registryHolder, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager, IOptions<RoomSettings> roomSettings)
	{
		this.roomSettings = roomSettings.Value;

		PublicRoomTypeManager publicManager = new(serviceProvider, dbContextFactory, navigatorManager);

		this.roomTypeManagers = new Dictionary<IRoomType, IRoomTypeManager>()
		{
			{ RoomTypes.Private.Get(registryHolder), new PrivateRoomTypeManager(serviceProvider, dbContextFactory, navigatorManager) },
			{ RoomTypes.PublicInstance.Get(registryHolder), publicManager },
			{ RoomTypes.PublicWorld.Get(registryHolder), publicManager },
		};

		this.privateRoomType = RoomTypes.Private.Get(registryHolder);
	}

	public IEnumerable<IRoom> LoadedRooms => this.roomTypeManagers.Values.SelectMany(m => m.LoadedRooms);

	public IEnumerable<TInstance> GetLoadedInstances<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> roomType) =>
		((IRoomTypeManager<TInstance, TInfo, TId>)this.roomTypeManagers[roomType]).LoadedInstances;

	public ValueTask<ICacheReference<TInstance>?> GetInstanceAsync<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> roomType, TId roomId, CancellationToken cancellationToken = default) =>
		((IRoomTypeManager<TInstance, TInfo, TId>)this.roomTypeManagers[roomType]).GetInstanceAsync(roomId, cancellationToken);

	public bool TryGetInstance<TInstance, TInfo, TId>(IRoomType<TInstance, TInfo, TId> roomType, TId roomId, [NotNullWhen(true)] out TInstance? instance) =>
		((IRoomTypeManager<TInstance, TInfo, TId>)this.roomTypeManagers[roomType]).TryGetInstance(roomId, out instance);

	public async Task LoadAsync(ILoadableServiceContext context, CancellationToken cancellationToken = default)
	{
		await context.RequestDependencyAsync<INavigatorSnapshot>(cancellationToken).ConfigureAwait(false);

		context.Commit(() =>
		{
			foreach (int roomId in this.roomSettings.ForceLoadPrivateRooms)
			{
				this.GetInstanceAsync(this.privateRoomType, roomId, cancellationToken).AsTask().ContinueWith(t =>
				{
					if (!t.IsCompletedSuccessfully || t.Result is null)
					{
						return t.Result;
					}

					this.forceLoadedRooms.AddOrUpdate(roomId, static (_, newValue) => newValue, static (_, oldValue, newValue) =>
					{
						oldValue.Dispose();

						return newValue;
					}, t.Result);

					return t.Result;
				}, cancellationToken);
			}
		});
	}
}
