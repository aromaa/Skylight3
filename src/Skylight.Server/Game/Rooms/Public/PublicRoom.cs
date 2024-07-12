using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Public;
using Skylight.API.Game.Rooms.Public;
using Skylight.Server.Game.Rooms.Map.Public;

namespace Skylight.Server.Game.Rooms.Public;

internal sealed class PublicRoom(RoomData roomData, IRoomLayout roomLayout) : Room(roomData, roomLayout), IPublicRoom
{
	public override IPublicRoomMap Map { get; } = new PublicRoomMap(roomLayout);

	public override Task LoadAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	internal override void DoTick()
	{
	}
}
