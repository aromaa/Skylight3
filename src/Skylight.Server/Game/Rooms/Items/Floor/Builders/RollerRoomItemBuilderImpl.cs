using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class RollerRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private IRollerFurniture? FurnitureValue { get; set; }

	public override RollerRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (IRollerFurniture)furniture;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out IRollerInteractionHandler? handler))
		{
			throw new Exception($"{typeof(IRollerInteractionHandler)} not found");
		}

		return new RollerRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
