using Skylight.API.Game.Inventory;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Private;

namespace Skylight.Server.Game.Navigator;

internal sealed class NavigatorSearch(INavigatorManager navigatorManager, IRoomManager roomManager, RoomActivityWorker roomActivityWorker) : INavigatorSearch
{
	private readonly INavigatorManager navigatorManager = navigatorManager;
	private readonly IRoomManager roomManager = roomManager;

	private readonly RoomActivityWorker roomActivityWorker = roomActivityWorker;

	public IEnumerable<IPrivateRoomInfo> PopularRooms
	{
		get
		{
			const int count = 50;

			HashSet<int> rooms = [];
			foreach (IPrivateRoom room in this.roomManager.LoadedPrivateRooms
				.Where(r => r.Info.UserCount > 0)
				.OrderByDescending(r => r.Info.UserCount)
				.ThenBy(r => r.Info.Id))
			{
				rooms.Add(room.Info.Id);

				yield return room.Info;
			}

			int i = rooms.Count;
			if (i >= count)
			{
				yield break;
			}

			foreach ((_, int roomId) in this.roomActivityWorker.Values)
			{
				if (rooms.Contains(roomId))
				{
					continue;
				}

				//TODO: Fix
				IPrivateRoomInfo? info = this.navigatorManager.GetPrivateRoomInfoAsync(roomId).GetAwaiter().GetResult();
				if (info is null)
				{
					continue;
				}

				yield return info;

				if (++i >= count)
				{
					yield break;
				}
			}
		}
	}
}
