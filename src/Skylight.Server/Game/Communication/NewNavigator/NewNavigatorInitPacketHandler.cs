using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.NewNavigator;
using Skylight.Protocol.Packets.Incoming.NewNavigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.NewNavigator;

namespace Skylight.Server.Game.Communication.NewNavigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class NewNavigatorInitPacketHandler<T> : UserPacketHandler<T>
	where T : INewNavigatorInitIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		user.SendAsync(new NavigatorMetaDataOutgoingPacket
		{
			TopLevelContexts = new List<TopLevelContext>
			{
				new TopLevelContext("official_view", Array.Empty<SavedSearchData>()),
				new TopLevelContext("hotel_view", Array.Empty<SavedSearchData>()),
				new TopLevelContext("roomads_view", Array.Empty<SavedSearchData>()),
				new TopLevelContext("myworld_view", Array.Empty<SavedSearchData>())
			}
		});

		user.SendAsync(new NavigatorLiftedRoomsOutgoingPacket(Array.Empty<LiftedRoomData>()));
		user.SendAsync(new NavigatorCollapsedCategoriesOutgoingPacket(Array.Empty<string>()));
		user.SendAsync(new NavigatorSavedSearchesOutgoingPacket(Array.Empty<SavedSearchData>()));
		user.SendAsync(new NewNavigatorPreferencesOutgoingPacket(0, 0, 100, 100, false, 0));
	}
}
