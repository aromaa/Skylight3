using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class MoveAvatarPacketHandler<T> : UserPacketHandler<T>
	where T : IMoveAvatarIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		Point2D location = new(packet.X, packet.Y);

		roomUnit.Room.PostTask(_ =>
		{
			if (!roomUnit.InRoom)
			{
				return;
			}

			roomUnit.PathfindTo(location);
		});
	}
}
