using System.Collections.Immutable;
using Skylight.API.Game.Rooms.Public;

namespace Skylight.Server.Game.Rooms.Public;

internal sealed class PublicRoomInstance(ImmutableArray<IPublicRoom> rooms) : IPublicRoomInstance
{
	public ImmutableArray<IPublicRoom> Rooms { get; } = rooms;
}
