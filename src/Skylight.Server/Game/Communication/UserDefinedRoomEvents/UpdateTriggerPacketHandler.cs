using System.Text;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Wired.Triggers;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.UserDefinedRoomEvents;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.UserDefinedRoomEvents;

namespace Skylight.Server.Game.Communication.UserDefinedRoomEvents;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class UpdateTriggerPacketHandler<T> : UserPacketHandler<T>
	where T : IUpdateTriggerIncomingPacket
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

		privateRoom.PostTask(room =>
		{
			if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item) || item is not IWiredTriggerRoomItem trigger)
			{
				return;
			}

			HashSet<IRoomItem> selectedItems = [];
			foreach (int selectedItemId in selectedItemIds)
			{
				if (!privateRoom.ItemManager.TryGetFloorItem(selectedItemId, out IFloorRoomItem? selectedItem))
				{
					continue;
				}

				selectedItems.Add(selectedItem);
			}

			if (trigger is IUnitSayTriggerRoomItem userSay)
			{
				userSay.Message = stringParameter;
			}
			else if (trigger is IUnitEnterRoomTriggerRoomItem enterRoom)
			{
				enterRoom.TriggerUsername = string.IsNullOrWhiteSpace(stringParameter) ? null : stringParameter;
			}
			else if (trigger is IUnitUseItemTriggerRoomItem useItem)
			{
				useItem.SelectedItems = selectedItems;
			}
			else if (trigger is IUnitWalkOffTriggerRoomItem walkOff)
			{
				walkOff.SelectedItems = selectedItems;
			}
			else if (trigger is IUnitWalkOnTriggerRoomItem walkOn)
			{
				walkOn.SelectedItems = selectedItems;
			}

			user.SendAsync(new WiredSaveSuccessOutgoingPacket());
		});
	}
}
