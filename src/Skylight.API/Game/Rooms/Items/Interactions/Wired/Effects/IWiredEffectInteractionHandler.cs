using Skylight.API.Game.Rooms.Items.Floor.Wired;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;

public interface IWiredEffectInteractionHandler : IRoomItemInteractionHandler
{
	public void OnPlace(IWiredEffectRoomItem effect);
	public void OnMove(IWiredEffectRoomItem effect, Point3D newPosition);
	public void OnRemove(IWiredEffectRoomItem effect);

	public void TriggerStack(IWiredRoomItem wired, IUserRoomUnit? cause = null);
}
