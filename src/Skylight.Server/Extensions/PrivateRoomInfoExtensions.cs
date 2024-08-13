using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Navigator;

namespace Skylight.Server.Extensions;

internal static class PrivateRoomInfoExtensions
{
	internal static GuestRoomData BuildGuestRoomData(this IPrivateRoomInfo info)
	{
		(IUserInfo owner, IRoomSettings settings) = info.Details;

		return new GuestRoomData(info.Id, owner.Id, owner.Username, info.Layout.Id, settings.Name, settings.Description, settings.Category.Value.Id, settings.Tags, settings.EntryMode.ToProtocol(),
			info.UserCount, settings.UsersMax, settings.TradeMode.ToProtocol(), 0, 0);
	}
}
