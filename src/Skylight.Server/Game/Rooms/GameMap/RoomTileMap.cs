using System.Buffers;
using CommunityToolkit.HighPerformance;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;
using Skylight.Server.Collections.Immutable;
using Skylight.Server.Game.Rooms.Layout;

namespace Skylight.Server.Game.Rooms.GameMap;

internal sealed class RoomTileMap : IRoomMap
{
	private static readonly Point2D[] directions =
		[new Point2D(0, 1), new Point2D(1, 0), new Point2D(0, -1), new Point2D(-1, 0), new Point2D(1, 1), new Point2D(-1, -1), new Point2D(1, -1), new Point2D(-1, 1)];

	internal Room Room { get; }

	public IRoomLayout Layout { get; }

	private readonly ImmutableArray2D<IRoomTile> tiles;

	internal RoomTileMap(Room room, IRoomLayout layout)
	{
		this.Room = room;

		this.Layout = layout;

		ImmutableArray2D<IRoomTile>.Builder builder = ImmutableArray2D.CreateBuilder<IRoomTile>(layout.Size.X, layout.Size.Y);

		for (int x = 0; x < layout.Size.X; x++)
		{
			for (int y = 0; y < layout.Size.Y; y++)
			{
				builder[x, y] = new RoomTile(this, new Point2D(x, y), ((RoomLayout)layout).Tiles[x, y]);
			}
		}

		this.tiles = builder.MoveToImmutable();
	}

	public bool IsValidLocation(Point2D point) => point.X < this.Layout.Size.X && point.Y < this.Layout.Size.Y;

	public IRoomTile GetTile(int x, int y) => this.tiles[x, y];
	public IRoomTile GetTile(Point2D point) => this.tiles[point.X, point.Y];

	public Stack<Point2D> PathfindTo(Point2D start, Point2D target, IRoomUnit unit)
	{
		//Check if valid target

		IRoomLayout layout = this.Room.Map.Layout;

		PriorityQueue<Point2D, int> points = new();
		(Point2D From, int Weight)[] array = ArrayPool<(Point2D, int)>.Shared.Rent(layout.Size.X * layout.Size.Y);

		Span2D<(Point2D From, int Weight)> data = new(array, layout.Size.X, layout.Size.Y);

		Array.Fill(array, (default, int.MaxValue));

		points.Enqueue(start, 0);
		data[start.X, start.Y] = (start, 0);

		Stack<Point2D> path = new();

		Span<Point2D> nextPoints = stackalloc Point2D[8];
		while (points.TryDequeue(out Point2D current, out int priority))
		{
			if (current == target)
			{
				path.Push(current);

				Point2D walkBack = data[current.X, current.Y].From;
				while (walkBack != start)
				{
					path.Push(walkBack);

					walkBack = data[walkBack.X, walkBack.Y].From;
				}

				break;
			}

			int currentWeight = data[current.X, current.Y].Weight + priority;

			for (int i = 0; i < this.GetNeighbors(current, ref nextPoints); i++)
			{
				Point2D neighbor = nextPoints[i];

				ref (Point2D From, int Weight) pathData = ref data[neighbor.X, neighbor.Y];

				int distance = Point2D.DistanceSquared(current, neighbor);
				int neighborWeight = currentWeight + distance;
				if (neighborWeight >= pathData.Weight)
				{
					continue;
				}

				pathData.From = current;
				pathData.Weight = neighborWeight;

				points.Enqueue(neighbor, priority + 1);
			}
		}

		ArrayPool<(Point2D, int)>.Shared.Return(array);

		return path;
	}

	private int GetNeighbors(Point2D point, ref Span<Point2D> buffer)
	{
		int i = 0;
		foreach (Point2D direction in RoomTileMap.directions)
		{
			Point2D newLocation = point + direction;
			if (!this.IsValidLocation(newLocation))
			{
				continue;
			}

			IRoomTile tile = this.GetTile(newLocation);
			if (tile.IsHole)
			{
				continue;
			}

			buffer[i++] = newLocation;
		}

		return i;
	}
}
