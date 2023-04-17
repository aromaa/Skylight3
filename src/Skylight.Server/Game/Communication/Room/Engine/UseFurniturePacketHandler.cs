using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class UseFurniturePacketHandler<T> : UserPacketHandler<T>
	where T : IUseFurnitureIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		roomUnit.Room.ScheduleTask(static (room, state) =>
		{
			if (!room.ItemManager.TryGetFloorItem(state.ItemId, out IFloorRoomItem? item))
			{
				return;
			}
		}, (ItemId: packet.Id, State: packet.State));
	}
}
