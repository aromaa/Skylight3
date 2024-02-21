using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	: IRoomManager
{
	private readonly IServiceProvider serviceProvider = serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;

	private readonly ConcurrentDictionary<int, IRoom> loadedRooms = new();

	public ICollection<IRoom> Rooms => this.loadedRooms.Values;

	public async ValueTask<IRoom?> GetRoomAsync(int id, CancellationToken cancellationToken)
	{
		if (this.loadedRooms.TryGetValue(id, out IRoom? room))
		{
			return room;
		}

		await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		IRoomInfo? roomInfo = await this.navigatorManager.GetRoomDataAsync(id, cancellationToken).ConfigureAwait(false);
		if (roomInfo is null)
		{
			return null;
		}

		ObjectFactory roomFactory = ActivatorUtilities.CreateFactory(typeof(Room), new Type[]
		{
			typeof(RoomData)
		});

		room = (Room)roomFactory(this.serviceProvider, new object[]
		{
			roomInfo
		});

		this.loadedRooms.TryAdd(id, room);

		await room.LoadAsync(cancellationToken).ConfigureAwait(false);

		return room;
	}

	public bool TryGetRoom(int id, [NotNullWhen(true)] out IRoom? room)
	{
		if (this.loadedRooms.TryGetValue(id, out room))
		{
			return true;
		}

		return false;
	}
}
