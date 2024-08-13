using Skylight.API.Game.Navigator;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms.Units.Private;

internal sealed class PrivateRoomUnitManager(PrivateRoom room, INavigatorManager navigatorManager) : RoomUnitManager
{
	protected override PrivateRoom Room { get; } = room;

	private readonly INavigatorManager navigatorManager = navigatorManager;

	private int pendingUnitActivityCounter;
	private int pendingUnitActivityCounterTimer = Environment.TickCount;

	public override void Tick()
	{
		base.Tick();

		this.pendingUnitActivityCounter += this.Units.Count();
		if (Environment.TickCount - this.pendingUnitActivityCounterTimer >= 30 * 1000)
		{
			if (this.pendingUnitActivityCounter > 0)
			{
				this.navigatorManager.PushRoomActivity(this.Room.Info.Id, this.pendingUnitActivityCounter);
			}

			this.pendingUnitActivityCounterTimer = Environment.TickCount;
			this.pendingUnitActivityCounter = 0;
		}
	}
}
