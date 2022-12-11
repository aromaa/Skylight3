namespace Skylight.Server.Game.Rooms.Layout;

internal sealed class RoomLayoutTile
{
	internal int Height { get; }

	internal RoomLayoutTile(int height)
	{
		this.Height = height;
	}

	public bool IsHole => this.Height == -100;
}
