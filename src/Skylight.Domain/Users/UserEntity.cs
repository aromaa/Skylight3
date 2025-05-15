﻿using Skylight.Domain.Badges;
using Skylight.Domain.Items;
using Skylight.Domain.Rooms.Private;

namespace Skylight.Domain.Users;

public class UserEntity
{
	public int Id { get; init; }

	public string Username { get; set; } = null!;
	public string Figure { get; set; } = null!;
	public string Gender { get; set; } = null!;
	public string Motto { get; set; } = null!;

	public DateTime LastOnline { get; set; }

	public ICollection<UserCurrenciesEntity> Currencies { get; set; } = new List<UserCurrenciesEntity>();
	public UserSettingsEntity? Settings { get; set; }

	public List<FloorItemEntity>? FloorItems { get; set; }
	public List<WallItemEntity>? WallItems { get; set; }

	public List<PrivateRoomEntity>? Rooms { get; set; }

	public List<UserBadgeEntity>? Badges { get; set; }
}
