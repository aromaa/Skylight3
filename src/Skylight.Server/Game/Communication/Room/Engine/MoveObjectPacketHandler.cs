using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Units;
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

		roomUnit.Room.ScheduleTask(new MoveObjectTask
		{
			RoomUnit = roomUnit,

			ItemId = packet.Id,

			Location = new Point2D(packet.X, packet.Y),
			Direction = packet.Direction,
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct MoveObjectTask : IRoomTask
	{
		internal IUserRoomUnit RoomUnit { get; init; }

		internal int ItemId { get; init; }

		internal Point2D Location { get; init; }
		internal int Direction { get; init; }

		public void Execute(IRoom room)
		{
			if (!this.RoomUnit.InRoom)
			{
				return;
			}

			if (!room.ItemManager.TryGetFloorItem(this.ItemId, out IFloorRoomItem? item))
			{
				return;
			}

			room.ItemManager.MoveItem(item, new Point3D(this.Location, room.ItemManager.GetPlacementHeight(item.Furniture, this.Location)), this.Direction);
		}
	}
}
