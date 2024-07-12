using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Effects;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Communication.UserDefinedRoomEvents;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class UpdateEffectPacketHandler<T> : UserPacketHandler<T>
	where T : IUpdateActionIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit || !privateRoom.IsOwner(user))
		{
			return;
		}

		int itemId = packet.ItemId;

		IList<int> selectedItemIds = packet.SelectedItems;
		IList<int> integerParameters = packet.IntegerParameters;
		string stringParameter = Encoding.UTF8.GetString(packet.StringParameter);

		int actionDelay = packet.ActionDelay;

		roomUnit.Room.PostTask(room =>
		{
			if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item) || item is not IWiredEffectRoomItem effect)
			{
				return;
			}

			HashSet<IRoomItem> selectedItems = [];
			foreach (int selectedItemId in selectedItemIds)
			{
				if (!privateRoom.ItemManager.TryGetItem(selectedItemId, out IRoomItem? selectedItem))
				{
					continue;
				}

				selectedItems.Add(selectedItem);
			}

			if (effect is IShowMessageEffectRoomItem showMessage)
			{
				showMessage.Message = stringParameter;
			}

			if (effect is ICycleItemStateRoomItem cycle)
			{
				cycle.SelectedItems = selectedItems;
			}
			else if (effect is ITeleportUnitEffectRoomItem teleport)
			{
				teleport.SelectedItems = selectedItems;
			}

			effect.EffectDelay = actionDelay;

			user.SendAsync(new WiredSaveSuccessOutgoingPacket());
		});
	}
}
