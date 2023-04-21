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

		Point2D location = new(packet.X, packet.Y);

		roomUnit.Room.ScheduleTask(static (r, state) =>
		{
			if (!state.RoomUnit.InRoom || state.RoomUnit.Moving)
			{
				return;
			}

			state.RoomUnit.LookTo(state.Location);
		}, (RoomUnit: roomUnit, Location: location));
	}
}
