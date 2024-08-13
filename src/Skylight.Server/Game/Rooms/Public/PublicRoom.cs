using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Map.Public;
using Skylight.API.Game.Rooms.Public;
using Skylight.API.Game.Rooms.Units;
using Skylight.Server.Game.Rooms.Map.Public;
using Skylight.Server.Game.Rooms.Units.Public;

namespace Skylight.Server.Game.Rooms.Public;

internal sealed class PublicRoom : Room, IPublicRoom
{
	public override IPublicRoomInfo Info { get; }
	public override IPublicRoomMap Map { get; }
	public override IRoomUnitManager UnitManager { get; }

	public PublicRoom(IPublicRoomInfo info, IRoomLayout roomLayout)
		: base(roomLayout)
	{
		this.Info = info;
		this.Map = new PublicRoomMap(roomLayout);

		this.UnitManager = new PublicRoomUnitManager(this);
	}

	public override Task LoadAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	internal override void DoTick()
	{
	}
}
