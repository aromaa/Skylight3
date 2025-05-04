using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class MyRoomsSearchPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager) : UserPacketHandler<T>
	where T : IMyRoomsSearchIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		using SkylightContext dbContext = this.dbContextFactory.CreateDbContext();

		List<GuestRoomData> rooms = [];

		foreach (int roomId in dbContext.PrivateRooms
			.Where(r => r.OwnerId == user.Profile.Id)
			.Select(r => r.Id))
		{
			IPrivateRoomInfo room = this.navigatorManager.GetPrivateRoomInfoAsync(roomId).GetAwaiter().GetResult()!;

			rooms.Add(room.BuildGuestRoomData());
		}

		user.SendAsync(new GuestRoomSearchResultOutgoingPacket(5, string.Empty, rooms));
	}
}
