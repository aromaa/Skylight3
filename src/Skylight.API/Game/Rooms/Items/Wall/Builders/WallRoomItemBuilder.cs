using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Wall.Builders;

public abstract class WallRoomItemBuilder : IFurnitureItemBuilder<IWallRoomItem>
{
	protected int ItemIdValue { get; set; }

	protected IRoom? RoomValue { get; set; }
	protected IUserInfo? OwnerValue { get; set; }

	protected Point2D LocationValue { get; set; }
	protected Point2D PositionValue { get; set; }

	public WallRoomItemBuilder ItemId(int itemId)
	{
		this.ItemIdValue = itemId;

		return this;
	}

	public abstract WallRoomItemBuilder Furniture(IWallFurniture furniture);

	public WallRoomItemBuilder Room(IRoom room)
	{
		this.RoomValue = room;

		return this;
	}

	public WallRoomItemBuilder Owner(IUserInfo owner)
	{
		this.OwnerValue = owner;

		return this;
	}

	public WallRoomItemBuilder Location(Point2D location)
	{
		this.LocationValue = location;

		return this;
	}

	public WallRoomItemBuilder Position(Point2D position)
	{
		this.PositionValue = position;

		return this;
	}

	public virtual WallRoomItemBuilder ExtraData(JsonDocument extraData)
	{
		return this;
	}

	public abstract IWallRoomItem Build();

	[MemberNotNull(nameof(this.RoomValue), nameof(this.OwnerValue))]
	protected virtual void CheckValid()
	{
		ArgumentOutOfRangeException.ThrowIfZero(this.ItemIdValue);
		ArgumentNullException.ThrowIfNull(this.RoomValue);
		ArgumentNullException.ThrowIfNull(this.OwnerValue);
	}
}
