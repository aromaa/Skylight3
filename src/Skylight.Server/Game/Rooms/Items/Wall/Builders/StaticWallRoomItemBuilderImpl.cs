using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms.Items.Wall;
using Skylight.API.Game.Rooms.Items.Wall.Builders;

namespace Skylight.Server.Game.Rooms.Items.Wall.Builders;

internal sealed class StaticWallRoomItemBuilderImpl
	: WallRoomItemBuilder, IFurnitureItemBuilder<IWallFurniture, IWallRoomItem>
{
	private IStaticWallFurniture? FurnitureValue { get; set; }

	public override StaticWallRoomItemBuilderImpl Furniture(IWallFurniture furniture)
	{
		this.FurnitureValue = (IStaticWallFurniture)furniture;

		return this;
	}

	public override IWallRoomItem Build()
	{
		this.CheckValid();

		return new StaticWallRoomItem(this.RoomValue, this.ItemIdValue, this.OwnerValue, this.FurnitureValue, this.LocationValue, this.PositionValue, 0);
	}

	[MemberNotNull(nameof(this.FurnitureValue))]
	protected override void CheckValid()
	{
		base.CheckValid();

		ArgumentNullException.ThrowIfNull(this.FurnitureValue);
	}
}
