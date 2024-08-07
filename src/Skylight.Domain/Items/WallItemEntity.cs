﻿using Skylight.Domain.Furniture;
using Skylight.Domain.Rooms.Private;
using Skylight.Domain.Users;

namespace Skylight.Domain.Items;

public class WallItemEntity
{
	public int Id { get; init; }
	public int UserId { get; set; }
	public UserEntity? User { get; set; }

	public int FurnitureId { get; set; }
	public WallFurnitureEntity? Furniture { get; set; }

	public int? RoomId { get; set; }
	public PrivateRoomEntity? Room { get; set; }

	public int LocationX { get; set; }
	public int LocationY { get; set; }

	public int PositionX { get; set; }
	public int PositionY { get; set; }

	public int Direction { get; set; }

	public WallItemDataEntity? Data { get; set; }
}
