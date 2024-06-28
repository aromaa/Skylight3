using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class UseFurniturePacketHandler<T> : UserPacketHandler<T>
	where T : IUseFurnitureIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		int itemId = packet.Id;
		int state = packet.State;

		roomUnit.Room.PostTask(room =>
		{
			if (!room.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item) || item is not IInteractableRoomItem interactable)
			{
				return;
			}

			if (interactable.Interact(roomUnit, state) && room.ItemManager.TryGetInteractionHandler(out IUnitUseItemTriggerInteractionHandler? handler))
			{
				handler.OnUse(roomUnit, interactable);
			}
		});
	}
}
