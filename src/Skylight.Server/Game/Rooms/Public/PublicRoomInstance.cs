using Skylight.API.Game.Rooms.Public;

namespace Skylight.Server.Game.Rooms.Public;

internal sealed class PublicRoomInstance(int id) : IPublicRoomInstance
{
	public int Id { get; } = id;
}
