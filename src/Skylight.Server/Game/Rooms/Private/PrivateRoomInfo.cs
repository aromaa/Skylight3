using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;

namespace Skylight.Server.Game.Rooms.Private;

internal sealed class PrivateRoomInfo(int id, IUserInfo owner, IRoomLayout layout, IRoomSettings settings) : RoomInfo(layout), IPrivateRoomInfo
{
	private RoomDetails details = new(owner, settings);

	public override int Id { get; } = id;

	public IUserInfo Owner
	{
		get => this.details.Owner;
		set => this.details.Owner = value;
	}

	public IRoomSettings Settings
	{
		get => this.details.Settings;
		set => this.details.Settings = value;
	}

	public (IUserInfo Owner, IRoomSettings Settings) Details
	{
		get => this.details.Tuple;
		set => this.details = new RoomDetails(value.Owner, value.Settings);
	}

	//Allows atomic owner & settings operations
	private sealed class RoomDetails(IUserInfo owner, IRoomSettings settings)
	{
		internal IUserInfo Owner { get; set; } = owner;
		internal IRoomSettings Settings { get; set; } = settings;

		internal (IUserInfo Owner, IRoomSettings Settings) Tuple => (this.Owner, this.Settings);
	}
}
