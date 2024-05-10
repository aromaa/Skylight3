using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class StaticFloorRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private IStaticFloorFurniture? FurnitureValue { get; set; }

	public override StaticFloorRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IStaticFloorFurniture)furniture;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		return new StaticFloorRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
