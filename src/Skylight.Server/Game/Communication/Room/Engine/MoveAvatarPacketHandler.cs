using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class MoveAvatarPacketHandler<T> : UserPacketHandler<T>
	where T : IMoveAvatarIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		roomUnit.Room.ScheduleTask(new MoveAvatarTask
		{
			RoomUnit = roomUnit,

			Location = new Point2D(packet.X, packet.Y)
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct MoveAvatarTask : IRoomTask
	{
		internal IUserRoomUnit RoomUnit { get; init; }

		internal Point2D Location { get; init; }

		public void Execute(IRoom room)
		{
			if (!this.RoomUnit.InRoom)
			{
				return;
			}

			this.RoomUnit.PathfindTo(this.Location);
		}
	}
}
