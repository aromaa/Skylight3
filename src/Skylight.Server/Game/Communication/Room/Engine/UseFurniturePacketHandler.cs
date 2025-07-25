﻿using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Registry;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class UseFurniturePacketHandler<T>(IRegistryHolder registryHolder) : UserPacketHandler<T>
	where T : IUseFurnitureIncomingPacket
{
	// TODO: Support other domains
	private readonly IRoomItemDomain normalRoomItemDomain = RoomItemDomains.Normal.Get(registryHolder);

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit)
		{
			return;
		}

		RoomItemId itemId = new(this.normalRoomItemDomain, packet.Id);
		int state = packet.State;

		roomUnit.Room.PostTask(room =>
		{
			if (!privateRoom.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item) || item is not IInteractableRoomItem interactable)
			{
				return;
			}

			if (interactable.Interact(roomUnit, state) && privateRoom.ItemManager.TryGetInteractionHandler(out IUnitUseItemTriggerInteractionHandler? handler))
			{
				handler.OnUse(roomUnit, interactable);
			}
		});
	}
}
