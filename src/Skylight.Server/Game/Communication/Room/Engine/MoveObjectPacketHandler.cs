using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class MoveObjectPacketHandler<T> : UserPacketHandler<T>
	where T : IMoveObjectIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		roomUnit.Room.ScheduleTask(static (room, state) =>
		{
			if (!state.RoomUnit.InRoom)
			{
				return;
			}

			if (!room.ItemManager.TryGetFloorItem(state.ItemId, out IFloorRoomItem? item))
			{
				return;
			}

			room.ItemManager.MoveItem(item, state.Location, state.Direction);
		}, (RoomUnit: roomUnit, ItemId: packet.Id, Location: new Point2D(packet.X, packet.Y), Direction: packet.Direction));
	}
}
