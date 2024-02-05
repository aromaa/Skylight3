using System.Runtime.CompilerServices;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Collections.Immutable;

namespace Skylight.Server.Game.Rooms.Items.Interactions;

internal sealed class RollerInteractionHandler : IRollerInteractionHandler, IRoomTask
{
	private readonly IRoom room;

	private readonly ImmutableArray2D<RollerRoomTile> tiles;

	internal RollerInteractionHandler(IRoom room)
	{
		this.room = room;

		ImmutableArray2D<RollerRoomTile>.Builder builder = ImmutableArray2D.CreateBuilder<RollerRoomTile>(room.Map.Layout.Size.X, room.Map.Layout.Size.Y);
		for (int x = 0; x < room.Map.Layout.Size.X; x++)
		{
			for (int y = 0; y < room.Map.Layout.Size.Y; y++)
			{
				builder[x, y] = new RollerRoomTile(this, room.Map.GetTile(x, y));
			}
		}

		this.tiles = builder.MoveToImmutable();

		room.ScheduleUpdateTask(this);
	}

	public void Execute(IRoom room)
	{
		HashSet<int> itemsMoved = new();
		foreach (RollerRoomTile rollerTile in Unsafe.As<ImmutableArray2D<RollerRoomTile>, RollerRoomTile[,]>(ref Unsafe.AsRef(in this.tiles)))
		{
			if (rollerTile.TargetTile is not { } targetTile || targetTile.RoomTile.HasRoomUnit)
			{
				continue;
			}

			IRollerRoomItem roller = rollerTile.Roller!;

			double z;
			if (targetTile.Roller is { } targetRoller)
			{
				z = targetRoller.Position.Z + targetRoller.Height;
			}
			else
			{
				z = roller.Position.Z;
			}

			foreach (IFloorRoomItem item in rollerTile.RoomTile.GetFloorItemsBetween(roller.Position.Z + roller.Height, double.MaxValue).ToArray())
			{
				if (!itemsMoved.Add(item.Id))
				{
					continue;
				}

				this.room.ItemManager.MoveItem(item, new Point3D(targetTile.Position.XY, z + (item.Position.Z - (roller.Position.Z + roller.Height))));
			}

			foreach (IRoomUnit roomTileUnit in rollerTile.RoomTile.Units)
			{
				if (roomTileUnit.Pathfinding)
				{
					continue;
				}

				roomTileUnit.PathfindTo(targetTile.Position.XY);
			}
		}

		room.ScheduleUpdateTask(this);
	}

	public bool CanPlaceItem(IFurniture furniture, Point2D location)
	{
		return true;
	}

	public void OnPlace(IRollerRoomItem roller)
	{
		this.tiles[roller.Position.X, roller.Position.Y].Add(roller);
	}

	public void OnRemove(IRollerRoomItem roller)
	{
		this.tiles[roller.Position.X, roller.Position.Y].Remove(roller);
	}

	private sealed class RollerRoomTile
	{
		private readonly RollerInteractionHandler handler;

		internal IRoomTile RoomTile { get; }

		private readonly SortedSet<IRollerRoomItem> rollers;

		internal IRollerRoomItem? Roller { get; private set; }
		internal RollerRoomTile? TargetTile { get; private set; }

		internal RollerRoomTile(RollerInteractionHandler handler, IRoomTile roomTile)
		{
			this.handler = handler;

			this.RoomTile = roomTile;

			this.rollers = new SortedSet<IRollerRoomItem>(RoomItemHeightComparer.Instance);
		}

		internal Point3D Position => this.RoomTile.Position;

		internal void Add(IRollerRoomItem roller)
		{
			this.rollers.Add(roller);

			this.Update(this.rollers.Min);
		}

		internal void Remove(IRollerRoomItem roller)
		{
			this.rollers.Remove(roller);

			this.Update(this.rollers.Min);
		}

		private void Update(IRollerRoomItem? roller)
		{
			this.Roller = roller;
			this.TargetTile = null;

			if (roller is null)
			{
				return;
			}

			Point2D targetLocation = roller.Position.XY + roller.Direction switch
			{
				0 => new Point2D(0, -1),
				2 => new Point2D(1, 0),
				4 => new Point2D(0, 1),
				6 => new Point2D(-1, 0),

				_ => default
			};

			if (!this.RoomTile.Map.IsValidLocation(targetLocation))
			{
				return;
			}

			IRoomTile targetTile = this.RoomTile.Map.GetTile(targetLocation);
			if (targetTile.IsHole)
			{
				return;
			}

			this.TargetTile = this.handler.tiles[targetLocation.X, targetLocation.Y];
		}
	}
}
