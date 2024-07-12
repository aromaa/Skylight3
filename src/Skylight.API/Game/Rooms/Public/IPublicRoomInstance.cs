using System.Collections.Immutable;

namespace Skylight.API.Game.Rooms.Public;

public interface IPublicRoomInstance
{
	public ImmutableArray<IPublicRoom> Rooms { get; }
}
