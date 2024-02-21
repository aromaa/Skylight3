using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Floor.Builders;

public abstract class FloorRoomItemBuilder : IFurnitureItemBuilder<IFloorRoomItem>
{
	protected int ItemIdValue { get; set; }

	protected IRoom? RoomValue { get; set; }
	protected IUserInfo? OwnerValue { get; set; }

	protected Point3D PositionValue { get; set; }
	protected int DirectionValue { get; set; }

	public FloorRoomItemBuilder ItemId(int itemId)
	{
		this.ItemIdValue = itemId;

		return this;
	}

	public abstract FloorRoomItemBuilder Furniture(IFloorFurniture furniture);

	public FloorRoomItemBuilder Room(IRoom room)
	{
		this.RoomValue = room;

		return this;
	}

	public FloorRoomItemBuilder Owner(IUserInfo owner)
	{
		this.OwnerValue = owner;

		return this;
	}

	public FloorRoomItemBuilder Position(Point3D position)
	{
		this.PositionValue = position;

		return this;
	}

	public FloorRoomItemBuilder Direction(int direction)
	{
		this.DirectionValue = direction;

		return this;
	}

	public virtual FloorRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		return this;
	}

	public abstract IFloorRoomItem Build();

	[MemberNotNull(nameof(this.RoomValue), nameof(this.OwnerValue))]
	protected virtual void CheckValid()
	{
		ArgumentOutOfRangeException.ThrowIfZero(this.ItemIdValue);
		ArgumentNullException.ThrowIfNull(this.RoomValue);
		ArgumentNullException.ThrowIfNull(this.OwnerValue);
	}
}
