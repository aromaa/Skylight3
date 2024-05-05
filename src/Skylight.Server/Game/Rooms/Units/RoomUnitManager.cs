using System.Globalization;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.API.Numerics;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Extensions;
using Skylight.Server.Game.Users;

namespace Skylight.Server.Game.Rooms.Units;

internal sealed class RoomUnitManager : IRoomUnitManager
{
	private readonly Room room;

	private readonly Dictionary<int, IRoomUnit> roomUnits;

	private readonly LinkedList<IRoomUnit> movingUnits;

	private int nextUnitId;

	internal RoomUnitManager(Room room)
	{
		this.room = room;

		this.roomUnits = [];

		this.movingUnits = new LinkedList<IRoomUnit>();

		this.nextUnitId = 1;
	}

	public IEnumerable<IRoomUnit> Units => this.roomUnits.Values;

	public void Tick()
	{
		if (this.movingUnits.Count > 0)
		{
			this.movingUnits.Shuffle();

			for (LinkedListNode<IRoomUnit>? node = this.movingUnits.First; node is not null;)
			{
				IRoomUnit roomUnit = node.Value;
				roomUnit.Tick();

				this.room.SendAsync(new UserUpdateOutgoingPacket(
				[
					new RoomUnitUpdateData(roomUnit.Id, roomUnit.Position.X, roomUnit.Position.Y, roomUnit.Position.Z, roomUnit.Rotation.X, roomUnit.Rotation.Y, roomUnit.Moving ? $"mv {roomUnit.NextStepPosition.X},{roomUnit.NextStepPosition.Y},{roomUnit.NextStepPosition.Z.ToString(CultureInfo.InvariantCulture)}" : string.Empty)
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
	}

	public IUserRoomUnit CreateUnit(IUser user)
	{
		RoomUnit unit = new(this, this.room, (User)user, this.nextUnitId++, new Point3D(this.room.Map.Layout.DoorLocation.X, this.room.Map.Layout.DoorLocation.Y, 0));

		this.AddUnit(unit);

		return unit;
	}

	public void AddUnit(IRoomUnit unit)
	{
		this.roomUnits.Add(unit.Id, unit);

		if (unit is IUserRoomUnit userUnit)
		{
			this.room.SendAsync(new UsersOutgoingPacket(
			[
				new RoomUnitData
				{
					IdentifierId = userUnit.User.Profile.Id,
					Name = userUnit.User.Profile.Username,
					Motto = userUnit.User.Profile.Motto,
					Figure = userUnit.User.Profile.Figure,
					RoomUnitId = userUnit.Id,
					X = unit.Position.X,
					Y = unit.Position.Y,
					Z = unit.Position.Z,
					Direction = unit.Rotation.X,
					Type = 1,
					Gender = userUnit.User.Profile.Gender,
					GroupId = 0,
					GroupStatus = 0,
					GroupName = string.Empty,
					SwimSuit = string.Empty,
					AchievementScore = 666,
					IsModerator = true
				}
			]));

			if (this.room.ItemManager.TryGetInteractionHandler(out IUnitEnterRoomTriggerInteractionHandler? handler))
			{
				handler.OnEnterRoom(userUnit);
			}
		}
	}

	public void RemoveUnit(IRoomUnit unit)
	{
		this.roomUnits.Remove(unit.Id);
		this.movingUnits.Remove(unit);

		IRoomTile lastTile = this.room.Map.GetTile(unit.Moving ? unit.NextStepPosition.XY : unit.Position.XY);
		lastTile.WalkOff(unit);

		this.room.SendAsync(new UserRemoveOutgoingPacket(unit.Id));
	}

	internal void Move(RoomUnit unit)
	{
		this.movingUnits.Remove(unit);
		this.movingUnits.AddLast(unit);
	}
}
