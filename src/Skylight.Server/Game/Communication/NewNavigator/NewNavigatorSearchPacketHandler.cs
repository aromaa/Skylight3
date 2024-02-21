using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Navigator;
using Skylight.Protocol.Packets.Data.NewNavigator;
using Skylight.Protocol.Packets.Incoming.NewNavigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.NewNavigator;

namespace Skylight.Server.Game.Communication.NewNavigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class NewNavigatorSearchPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : INewNavigatorSearchIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		string searchCode = Encoding.UTF8.GetString(packet.SearchCode);
		string filtering = Encoding.UTF8.GetString(packet.Filtering);

		List<GuestRoomData> rooms = new();

		if (searchCode == "myworld_view")
		{
			using SkylightContext dbContext = this.dbContextFactory.CreateDbContext();

			foreach (RoomEntity room in dbContext.Rooms.AsNoTracking()
						 .Include(r => r.Owner)
						 .Where(r => r.OwnerId == 1))
			{
				rooms.Add(new GuestRoomData(room.Id, room.Name, room.Owner!.Username, room.LayoutId, 0));
			}
		}

		user.SendAsync(new NavigatorSearchResultBlocksOutgoingPacket
		{
			SearchCode = searchCode,
			Filtering = filtering,

			Results = new List<SearchResultData>
			{
				new SearchResultData(searchCode, filtering, 0, false, 0, rooms)
			}
		});
	}
}
