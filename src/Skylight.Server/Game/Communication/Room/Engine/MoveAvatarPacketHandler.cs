using Net.Communication.Attributes;
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

		Point2D location = new(packet.X, packet.Y);

		roomUnit.Room.ScheduleTask(static (_, state) =>
		{
			if (!state.RoomUnit.InRoom)
			{
				return;
			}

			state.RoomUnit.PathfindTo(state.Location);
		}, (RoomUnit: roomUnit, Location: location));
	}
}
