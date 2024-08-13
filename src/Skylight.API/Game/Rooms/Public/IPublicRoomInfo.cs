namespace Skylight.API.Game.Rooms.Public;

public interface IPublicRoomInfo : IRoomInfo
{
	public IPublicRoomInstance Instance { get; }

	public int WorldId { get; }
}
