using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Incoming.Room.Avatar;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Avatar;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class LookToPacketHandler<T> : UserPacketHandler<T>
	where T : ILookToIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		roomUnit.Room.ScheduleTask((r, state) =>
		{
			if (!state.RoomUnit.InRoom || roomUnit.Moving)
			{
				return;
			}

			roomUnit.LookTo(new Point2D(state.Packet.X, state.Packet.Y));
		}, (RoomUnit: roomUnit, Packet: packet));
	}
}
