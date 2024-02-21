using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class MultiStateFloorRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private IMultiStateFloorFurniture? FurnitureValue { get; set; }

	public override MultiStateFloorRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IMultiStateFloorFurniture)furniture;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		return new MultiStateFloorRoomItem<IMultiStateFloorFurniture>(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
