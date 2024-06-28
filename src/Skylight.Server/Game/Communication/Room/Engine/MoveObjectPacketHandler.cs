using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class MoveObjectPacketHandler<T> : UserPacketHandler<T>
	where T : IMoveObjectIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		int itemId = packet.Id;

		Point2D location = new(packet.X, packet.Y);
		int direction = packet.Direction;

		roomUnit.Room.PostTask(room =>
		{
			if (!roomUnit.InRoom || !room.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item))
			{
				return;
			}

			Point3D position = new(location, room.ItemManager.GetPlacementHeight(item.Furniture, location, direction));
			if (!room.ItemManager.CanMoveItem(item, position, direction, user))
			{
				return;
			}

			room.ItemManager.MoveItem(item, location, direction);
		});
	}
}
