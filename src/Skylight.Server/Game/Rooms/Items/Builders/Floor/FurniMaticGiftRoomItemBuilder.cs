using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Data;
using Skylight.Server.Game.Rooms.Items.Floor;

namespace Skylight.Server.Game.Rooms.Items.Builders.Floor;

internal sealed class FurniMaticGiftRoomItemBuilder : FloorItemBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftRoomItem, FurniMaticGiftRoomItemBuilder, FurniMaticGiftRoomItemBuilder>,
	IFurniMaticGiftRoomItemDataBuilder<IFurniMaticGiftFurniture, IFurniMaticGiftRoomItem, FurniMaticGiftRoomItemBuilder, FurniMaticGiftRoomItemBuilder>
{
	private DateTime RecycledAtValue { get; set; }

	public IFurniMaticGiftRoomItemDataBuilder Recycled(DateTime recycledAt)
	{
		this.RecycledAtValue = recycledAt;

		return this;
	}

	public override IFurniMaticGiftRoomItem Build()
	{
		this.CheckValid();

		DateTime recycledAt = this.RecycledAtValue;
		if (recycledAt == default)
		{
			if (this.ExtraDataValue is not null)
			{
				this.RecycledAtValue = this.ExtraDataValue.RootElement.GetDateTime();
			}
			else
			{
				throw new InvalidOperationException("You must provide recycled at or extra data");
			}
		}

		return new FurniMaticGiftRoomItem(this.RoomValue, this.IdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, recycledAt);
	}
}
