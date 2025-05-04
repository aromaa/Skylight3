using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class MoveObjectPacketHandler<T> : UserPacketHandler<T>
	where T : IMoveObjectIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit)
		{
			return;
		}

		int itemId = packet.Id;

		Point2D location = new(packet.X, packet.Y);
		int direction = packet.Direction;

		privateRoom.PostTask(_ =>
		{
			if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetFloorItem(itemId, out IFloorRoomItem? item))
			{
				return;
			}

			Point3D position = new(location, privateRoom.ItemManager.GetPlacementHeight(item.Furniture, location, direction));
			if (!privateRoom.ItemManager.CanMoveItem(item, position, direction, user))
			{
				return;
			}

			privateRoom.ItemManager.MoveItem(item, location, direction);
		});
	}
}
