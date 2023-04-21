using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
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
		private readonly IDbContextFactory<SkylightContext> dbContextFactory;

		public LookToPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory)
		{
			this.dbContextFactory = dbContextFactory;
		}

		internal override void Handle(IUser user, in T packet)
		{
			if (user.RoomSession?.Unit is not IUserRoomUnit roomUnit)
			{
				return;
			}

			if (!roomUnit.Moving)
			{
				roomUnit.LookToAsync(packet.X, packet.Y);

				user.SendAsync(new UserUpdateOutgoingPacket(new List<RoomUnitUpdateData>
				{
					new(roomUnit.Id, roomUnit.Position.X, roomUnit.Position.Y, roomUnit.Position.Z, roomUnit.BodyRotation, roomUnit.HeadRotation, string.Empty)
				}));
			}
		}
	}
}
