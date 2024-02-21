using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Floor.Builders;
using Skylight.API.Game.Rooms.Items.Interactions;

namespace Skylight.Server.Game.Rooms.Items.Floor.Builders;

internal sealed class SoundMachineRoomItemBuilderImpl
	: FloorRoomItemBuilder
{
	private ISoundMachineFurniture? FurnitureValue { get; set; }

	public override SoundMachineRoomItemBuilderImpl Furniture(IFloorFurniture furniture)
	{
		this.FurnitureValue = (ISoundMachineFurniture)furniture;

		return this;
	}

	public override IFloorRoomItem Build()
	{
		this.CheckValid();

		if (!this.RoomValue.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler))
		{
			throw new Exception($"{typeof(ISoundMachineInteractionManager)} not found");
		}

		return new SoundMachineRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.PositionValue, this.DirectionValue, handler);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
