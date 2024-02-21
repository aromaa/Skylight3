using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class BasicFloorRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private IBasicFloorFurniture? FurnitureValue { get; set; }

	public override BasicFloorRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IBasicFloorFurniture)furniture;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		return new BasicFloorRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
