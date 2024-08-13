using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Data.NewNavigator;
using Skylight.Protocol.Packets.Incoming.NewNavigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.NewNavigator;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.NewNavigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class NewNavigatorSearchPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager, IRoomManager roomManager) : UserPacketHandler<T>
	where T : INewNavigatorSearchIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;
	private readonly IRoomManager roomManager = roomManager;

	internal override void Handle(IUser user, in T packet)
	{
		string searchCode = Encoding.UTF8.GetString(packet.SearchCode);
		string filtering = Encoding.UTF8.GetString(packet.Filtering);

		List<GuestRoomData> rooms = [];

		if (searchCode == "myworld_view")
		{
			using SkylightContext dbContext = this.dbContextFactory.CreateDbContext();

			foreach (int roomId in dbContext.PrivateRooms
				.Where(r => r.OwnerId == user.Profile.Id)
				.Select(r => r.Id))
			{
				IPrivateRoomInfo room = this.navigatorManager.GetPrivateRoomInfoAsync(roomId).GetAwaiter().GetResult()!;

				rooms.Add(room.BuildGuestRoomData());
			}
		}
		else if (searchCode == "hotel_view")
		{
			foreach (IPrivateRoom room in this.roomManager.LoadedPrivateRooms.OrderByDescending(r => r.Info.UserCount))
			{
				rooms.Add(room.Info.BuildGuestRoomData());
			}
		}

		user.SendAsync(new NavigatorSearchResultBlocksOutgoingPacket
		{
			SearchCode = searchCode,
			Filtering = filtering,

			Results =
			[
				new SearchResultData(searchCode, filtering, 0, false, 0, rooms)
			]
		});
	}
}
