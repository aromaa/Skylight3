using System.Globalization;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Rooms.Private;
using Skylight.Server.Game.Users;

namespace Skylight.Server.Game.Rooms.Units;

internal abstract class RoomUnitManager : IRoomUnitManager
{
	protected abstract Room Room { get; }

	private readonly Dictionary<int, IRoomUnit> roomUnits = [];
	private readonly LinkedList<IRoomUnit> movingUnits = [];

	private readonly Dictionary<int, UserTracker> trackers = [];

	private int nextUnitId = 1;

	public IEnumerable<IRoomUnit> Units => this.roomUnits.Values;

	public virtual void Tick()
	{
		if (this.movingUnits.Count > 0)
		{
			this.movingUnits.Shuffle();

			for (LinkedListNode<IRoomUnit>? node = this.movingUnits.First; node is not null;)
			{
				IRoomUnit roomUnit = node.Value;
				roomUnit.Tick();

				this.Room.SendAsync(new UserUpdateOutgoingPacket(
				[
					new RoomUnitUpdateData(roomUnit.Id, ((IUserRoomUnit)roomUnit).User.Info.Username, roomUnit.Position.X, roomUnit.Position.Y, roomUnit.Position.Z, roomUnit.Rotation.X, roomUnit.Rotation.Y, roomUnit.Moving ? $"mv {roomUnit.NextStepPosition.X},{roomUnit.NextStepPosition.Y},{roomUnit.NextStepPosition.Z.ToString(CultureInfo.InvariantCulture)}" : string.Empty)
				]));

				if (!roomUnit.Moving)
				{
					//Because we are removing a node, we need to get the next node first
					(LinkedListNode<IRoomUnit> current, node) = (node, node.Next);

					this.movingUnits.Remove(current);
				}
				else
				{
					node = node.Next;
				}
			}
		}

		this.Room.Info.UserCount = this.roomUnits.Count;
	}

	public IUserRoomUnit CreateUnit(IUser user)
	{
		RoomUnit unit = new(this, this.Room, (User)user, this.nextUnitId++, new Point3D(this.Room.Map.Layout.DoorLocation.X, this.Room.Map.Layout.DoorLocation.Y, 0));

		this.AddUnit(unit);

		return unit;
	}

	public void AddUnit(IRoomUnit unit)
	{
		this.roomUnits.Add(unit.Id, unit);

		if (unit is IUserRoomUnit userUnit)
		{
			UserTracker tracker = new(userUnit);

			this.trackers.Add(unit.Id, tracker);

			tracker.Add();

			if (this.Room is PrivateRoom privateRoom && privateRoom.ItemManager.TryGetInteractionHandler(out IUnitEnterRoomTriggerInteractionHandler? handler))
			{
				handler.OnEnterRoom(userUnit);
			}
		}
	}

	public void RemoveUnit(IRoomUnit unit)
	{
		IRoomTile lastTile = this.Room.Map.GetTile(unit.Moving ? unit.NextStepPosition.XY : unit.Position.XY);
		lastTile.WalkOff(unit);

		this.roomUnits.Remove(unit.Id);
		this.movingUnits.Remove(unit);

		if (this.trackers.Remove(unit.Id, out UserTracker? tracker))
		{
			tracker.Remove();
		}
	}

	internal void Move(RoomUnit unit)
	{
		this.movingUnits.Remove(unit);
		this.movingUnits.AddLast(unit);
	}

	private sealed class UserTracker(IUserRoomUnit unit)
	{
		private readonly IUserRoomUnit unit = unit;

		internal void Add()
		{
			IUserInfoView view = this.unit.User.Info.Snapshot;

			this.unit.Room.SendAsync(new UsersOutgoingPacket(
			[
				new RoomUnitData
				{
					IdentifierId = view.Id,
					Name = view.Username,
					Motto = view.Motto,
					Figure = view.Avatar.Data.ToString(),
					RoomUnitId = this.unit.Id,
					X = this.unit.Position.X,
					Y = this.unit.Position.Y,
					Z = this.unit.Position.Z,
					Direction = this.unit.Rotation.X,
					Type = 1,
					Gender = view.Avatar.Sex.ToNetwork(),
					GroupId = 0,
					GroupStatus = 0,
					GroupName = string.Empty,
					SwimSuit = string.Empty,
					AchievementScore = 666,
					IsModerator = true
				}
			]));

			IUserInfoEvents events = this.unit.User.Info.Events(view);
			events.AvatarChanged += this.OnUserChanged;
			events.MottoChanged += this.OnUserChanged;
		}

		internal void Remove()
		{
			this.unit.User.Info.AvatarChanged -= this.OnUserChanged;
			this.unit.User.Info.MottoChanged -= this.OnUserChanged;

			this.unit.Room.SendAsync(new UserRemoveOutgoingPacket(this.unit.Id, this.unit.User.Info.Username));
		}

		private void OnUserChanged(object? sender, object e)
		{
			IUserInfo info = (IUserInfo)sender!;

			this.unit.Room.SendAsync(new UserChangeOutgoingPacket(this.unit.Id, info.Avatar.Data.ToString(), info.Avatar.Sex.ToNetwork(), info.Motto, 666));
		}
	}
}
