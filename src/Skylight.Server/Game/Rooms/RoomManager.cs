using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.Domain.Rooms;
using Skylight.Infrastructure;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomManager : IRoomManager
{
	private readonly IServiceProvider serviceProvider;

	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly INavigatorManager navigatorManager;

	private readonly ConcurrentDictionary<int, IRoom> loadedRooms;

	public RoomManager(IServiceProvider serviceProvider, IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	{
		this.serviceProvider = serviceProvider;

		this.dbContextFactory = dbContextFactory;

		this.navigatorManager = navigatorManager;

		this.loadedRooms = new ConcurrentDictionary<int, IRoom>();
	}

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
