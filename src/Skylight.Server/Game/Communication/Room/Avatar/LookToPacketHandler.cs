using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Incoming.Room.Avatar;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;

namespace Skylight.Server.Game.Communication.Room.Avatar
{
	[PacketManagerRegister(typeof(AbstractGamePacketManager))]
	internal sealed class LookToPacketHandler<T> : UserPacketHandler<T>
		where T : ILookToIncomingPacket
	{
		internal override void Handle(IUser user, in T packet)
		{
			if (user.RoomSession?.Unit is not IUserRoomUnit roomUnit)
			{
				return;
			}

			if (!roomUnit.Moving)
			{
				roomUnit.LookTo(packet.X, packet.Y);

				user.SendAsync(new UserUpdateOutgoingPacket(new List<RoomUnitUpdateData>
				{
					new(roomUnit.Id, roomUnit.Position.X, roomUnit.Position.Y, roomUnit.Position.Z, roomUnit.Rotation.Y, roomUnit.Rotation.X, string.Empty)
				}));
			}
		}
	}
}
