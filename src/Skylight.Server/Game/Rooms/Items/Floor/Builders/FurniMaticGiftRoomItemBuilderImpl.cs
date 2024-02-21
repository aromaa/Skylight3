using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class FurniMaticGiftRoomItemBuilderImpl
	: FurniMaticGiftRoomItemBuilder
{
	private IFurniMaticGiftFurniture? FurnitureValue { get; set; }

	public override FurniMaticGiftRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IFurniMaticGiftFurniture)furniture;

		return this;
	}

	public override FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		this.RecycledAtValue = extraData.RootElement.GetDateTime();

		return this;
	}

	public override IFurniMaticGiftRoomItem Build()
	{
		this.CheckValid();

		return new FurniMaticGiftRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, this.RecycledAtValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
		ArgumentOutOfRangeException.ThrowIfEqual(this.RecycledAtValue, default);
	}
}
