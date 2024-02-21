namespace Skylight.API.Game.Rooms.Items.Floor.Builders;

public abstract class FurniMaticGiftRoomItemBuilder
	: FloorRoomItemBuilder
{
	protected DateTime RecycledAtValue { get; set; }

	public FurniMaticGiftRoomItemBuilder RecycledAt(DateTime recycledAt)
	{
		this.RecycledAtValue = recycledAt;

		return this;
	}

	public abstract override IFurniMaticGiftRoomItem Build();
}
