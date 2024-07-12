using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Private;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class MyRoomsSearchPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : IMyRoomsSearchIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		using SkylightContext dbContext = this.dbContextFactory.CreateDbContext();

		List<GuestRoomData> rooms = [];

		foreach (PrivateRoomEntity room in dbContext.PrivateRooms
					 .AsNoTracking()
					 .Include(r => r.Owner)
					 .Where(r => r.OwnerId == user.Profile.Id))
		{
			rooms.Add(new GuestRoomData(room.Id, room.Name, room.Owner!.Username, room.LayoutId, 0));
		}

		user.SendAsync(new GuestRoomSearchResultOutgoingPacket(5, string.Empty, rooms));
	}
}
