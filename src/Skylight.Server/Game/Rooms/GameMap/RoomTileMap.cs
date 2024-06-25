using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

	private static readonly int[] sums = RoomTileMap.SumsTo(100);

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
				builder[x, y] = new RoomTile(room, this, new Point2D(x, y), ((RoomLayout)layout).Tiles[x, y]);
			}
		}

		this.tiles = builder.MoveToImmutable();
	}

	public bool IsValidLocation(Point2D point) => (uint)point.X < this.Layout.Size.X && (uint)point.Y < this.Layout.Size.Y;

	public IRoomTile GetTile(int x, int y) => this.tiles[x, y];
	public IRoomTile GetTile(Point2D point) => this.tiles[point.X, point.Y];

	public Stack<Point2D> PathfindTo(Point3D start, Point3D target, IRoomUnit unit)
	{
		//Check if valid target

		IRoomLayout layout = this.Room.Map.Layout;

		PointData[] bookkeepingArray = ArrayPool<PointData>.Shared.Rent(layout.Size.X * layout.Size.Y);
		Array.Fill(bookkeepingArray, new PointData());

		using GameMapData gameMap = new(bookkeepingArray, layout.Size.X, layout.Size.Y);

		PriorityQueue<Point3D, int> points = new();
		points.Enqueue(start, 0);
		gameMap.Get(start.X, start.Y, start.Z) = (start, 0);

		NeighborsBuffer nextPoints = default;
		while (points.TryDequeue(out Point3D current, out int priority))
		{
			ref readonly (Point3D From, int Weight) data = ref gameMap[current.X, current.Y, current.Z];

			if (double.IsNaN(target.Z) ? current.XY == target.XY : current == target)
			{
				Stack<Point2D> path = new();
				path.Push(current.XY);

				Point3D walkBack = data.From;
				while (walkBack != start)
				{
					path.Push(walkBack.XY);

					walkBack = gameMap[walkBack.X, walkBack.Y, walkBack.Z].From;
				}

				return path;
			}

			int currentWeight = data.Weight + priority;

			for (int i = 0; i < this.GetNeighbors(current.XY, nextPoints); i++)
			{
				IRoomTile neighbor = nextPoints[i];

				double? nextZ = neighbor.GetStepHeight(current.Z);
				if (nextZ is null)
				{
					continue;
				}

				double difference = nextZ.Value - current.Z;
				if (difference > 2)
				{
					//continue;
				}

				Point2D neighborPosition = neighbor.Position.XY;

				int distance = Point2D.DistanceSquared(current.XY, neighborPosition);
				if (difference < -2)
				{
					distance += RoomTileMap.sums[Math.Min(Math.Abs((int)(difference / 0.1)), RoomTileMap.sums.Length - 1)];
				}

				ref (Point3D From, int Weight) pathData = ref gameMap.Get(neighborPosition.X, neighborPosition.Y, nextZ.Value);

				int neighborWeight = currentWeight + distance;
				if (neighborWeight >= pathData.Weight)
				{
					continue;
				}

				pathData.From = current;
				pathData.Weight = neighborWeight;

				points.Enqueue(new Point3D(neighborPosition, nextZ.Value), priority + distance);
			}
		}

		return [];
	}

	private int GetNeighbors(Point2D point, Span<IRoomTile> buffer)
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

			buffer[i++] = tile;
		}

		return i;
	}

	private static int[] SumsTo(int x)
	{
		int[] values = new int[x + 1];

		int value = 0;
		for (int i = 0; i <= x; i++)
		{
			value += i;

			values[i] = value;
		}

		return values;
	}

	private ref struct GameMapData(PointData[] data, int height, int width)
	{
		private readonly PointData[] dataArray = data;
		private readonly Span2D<PointData> data = new(data, height, width);

		private ArrayHolder arrays;
		private List<(Point3D From, int Weight)[]>? arraysList;
		private int arrayCount;

		internal ref readonly (Point3D From, int Weight) this[int x, int y, double z]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				(Point3D From, int Weight)[] array = this.data[x, y].GetArray(z);

				return ref new Span2D<(Point3D From, int Weight)>(array, this.data.Height, this.data.Width)[x, y];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref (Point3D From, int Weight) Get(int x, int y, double z)
		{
			(Point3D From, int Weight)[] array = this.data[x, y].GetOrCreateArray(z, ref this);

			return ref new Span2D<(Point3D From, int Weight)>(array, this.data.Height, this.data.Width)[x, y];
		}

		[UnscopedRef]
		private Span<(Point3D From, int Weight)[]> Arrays => this.arrays;

		internal (Point3D From, int Weight)[] GetNextArray(int index)
		{
			if (index < this.arrayCount)
			{
				return this.arraysList is null
					? this.arrays[index]
					: this.arraysList[index];
			}

			(Point3D From, int Weight)[] array = ArrayPool<(Point3D From, int Weight)>.Shared.Rent((int)this.data.Length);

			Array.Fill(array, (default, int.MaxValue));

			if (this.arraysList is null)
			{
				if (index < this.Arrays.Length)
				{
					return this.arrays[this.arrayCount++] = array;
				}
				else
				{
					this.arraysList = [.. this.Arrays]; //Don't use the inline array directly https://github.com/dotnet/roslyn/issues/70708
					this.arraysList.Add(array);
				}
			}
			else
			{
				this.arraysList.Add(array);
			}

			this.arrayCount++;

			return array;
		}

		public void Dispose()
		{
			ArrayPool<PointData>.Shared.Return(this.dataArray);

			if (this.arraysList is null)
			{
				for (int i = 0; i < this.arrayCount; i++)
				{
					ArrayPool<(Point3D From, int Weight)>.Shared.Return(this.arrays[i]);
				}
			}
			else
			{
				foreach ((Point3D From, int Weight)[] array in this.arraysList)
				{
					ArrayPool<(Point3D From, int Weight)>.Shared.Return(array);
				}
			}
		}
	}

	private struct PointData
	{
		internal Dictionary<double, (Point3D From, int Weight)[]>? Overflow { get; set; }

		private int index;

		private IndexHolder indexes;
		private ArrayHolder arrays;

		public PointData()
		{
			this.Indexes.Fill(double.NaN);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal (Point3D From, int Weight)[] GetArray(double value)
		{
			return this.Overflow is null
				? this.arrays[this.Indexes.IndexOf(value)]
				: this.Overflow[value];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal (Point3D From, int Weight)[] GetOrCreateArray(double value, ref GameMapData gameMap)
		{
			if (this.Overflow is null)
			{
				int index = this.Indexes.IndexOf(value);
				if (index != -1)
				{
					return this.arrays[index];
				}

				(Point3D From, int Weight)[] array = gameMap.GetNextArray(this.index);

				if (this.index < this.Indexes.Length)
				{
					(this.indexes[this.index], this.arrays[this.index], this.index) = (value, array, this.index + 1);
				}
				else
				{
					Dictionary<double, (Point3D From, int Weight)[]> dictionary = new()
					{
						[value] = array
					};

					for (int i = 0; i < 5; i++)
					{
						dictionary[this.indexes[i]] = this.arrays[i];
					}

					(this.Overflow, this.index) = (dictionary, this.index + 1);
				}

				return array;
			}

			if (this.Overflow.TryGetValue(value, out (Point3D From, int Weight)[]? arrays))
			{
				return arrays;
			}

			return this.Overflow[value] = gameMap.GetNextArray(this.index++);
		}

		[UnscopedRef]
		internal Span<double> Indexes => this.indexes;
	}

	[InlineArray(5)]
	private struct IndexHolder
	{
		private double z;
	}

	[InlineArray(5)]
	private struct ArrayHolder
	{
		private (Point3D From, int Weight)[] array;
	}

	[InlineArray(8)]
	private struct NeighborsBuffer
	{
		private IRoomTile tile;
	}
}
