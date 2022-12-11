using Skylight.API.Game.Rooms;

namespace Skylight.Server.Game.Rooms;

internal sealed class RoomFlatCat : IRoomFlatCat
{
	public int Id { get; }
	public string Caption { get; }

	internal RoomFlatCat(int id, string caption)
	{
		this.Id = id;
		this.Caption = caption;
	}
}
