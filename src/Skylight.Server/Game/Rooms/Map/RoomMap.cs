using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Rooms.Map;

internal abstract class RoomMap : IRoomMap
{
	private static readonly (Point2D, PathfinderEdge.EdgeDirection)[] directions =
	[
		(new Point2D(0, 1), PathfinderEdge.EdgeDirection.North),
		(new Point2D(1, 0), PathfinderEdge.EdgeDirection.East),
		(new Point2D(0, -1), PathfinderEdge.EdgeDirection.South),
		(new Point2D(-1, 0), PathfinderEdge.EdgeDirection.West),
		(new Point2D(1, 1), PathfinderEdge.EdgeDirection.NorthEast),
		(new Point2D(1, -1), PathfinderEdge.EdgeDirection.SouthEast),
		(new Point2D(-1, -1), PathfinderEdge.EdgeDirection.SouthWest),
		(new Point2D(-1, 1), PathfinderEdge.EdgeDirection.NorthWest)
	];

	private static readonly int[] sums = RoomMap.SumsTo(100);

	public IRoomLayout Layout { get; }

	internal RoomMap(IRoomLayout layout)
	{
		this.Layout = layout;
	}

	public bool IsValidLocation(Point2D point) => (uint)point.X < this.Layout.Size.X && (uint)point.Y < this.Layout.Size.Y;

	public abstract IRoomTile GetTile(int x, int y);
	public abstract IRoomTile GetTile(Point2D point);

	public Stack<Point2D> PathfindTo(Point3D start, Point3D target, IRoomUnit unit)
	{
		//Check if valid target

		IRoomLayout layout = this.Layout;

		using PathfinderData pathfinderData = new(layout.Size.X, layout.Size.Y);

		PriorityQueue<Point3D, int> points = new();
		points.Enqueue(start, 0);
		pathfinderData.Get(start.X, start.Y, start.Z) = new PathfinderEdge(0);

		PathfinderData.NeighborsBuffer nextPoints = default;
		while (points.TryDequeue(out Point3D current, out int priority))
		{
			if (double.IsNaN(target.Z) ? current.XY == target.XY : current == target)
			{
				ref readonly PathfinderEdge data = ref pathfinderData[current.X, current.Y, current.Z];

				Stack<Point2D> path = new();
				path.Push(current.XY);

				Point3D walkBack = data.Backtrace(current.XY);
				while (walkBack != start)
				{
					path.Push(walkBack.XY);

					PathfinderEdge walkBackEdge = pathfinderData[walkBack.X, walkBack.Y, walkBack.Z];
					walkBack = walkBackEdge.Backtrace(walkBack.XY);
				}

				return path;
			}

			for (int i = 0; i < this.GetNeighbors(current.XY, nextPoints); i++)
			{
				(IRoomTile neighborTile, PathfinderEdge.EdgeDirection neighborDirection) = nextPoints[i];

				double? nextZ = neighborTile.GetStepHeight(current.Z);
				if (nextZ is null)
				{
					continue;
				}

				double difference = nextZ.Value - current.Z;
				if (difference > 2)
				{
					//continue;
				}

				Point2D neighborPosition = neighborTile.Position.XY;

				int distance = 1;
				if (difference < -2)
				{
					distance = Point2D.DistanceSquared(current.XY, neighborPosition) * RoomMap.sums[Math.Min(Math.Abs((int)(difference / 0.1)), RoomMap.sums.Length - 1)];
				}

				int neighborWeight = priority + distance;

				ref PathfinderEdge pathData = ref pathfinderData.Get(neighborPosition.X, neighborPosition.Y, nextZ.Value);
				if (pathData.TrySuggestCandidate(neighborWeight, neighborDirection, current.Z))
				{
					points.Enqueue(new Point3D(neighborPosition, nextZ.Value), neighborWeight);
				}
			}
		}

		return [];
	}

	private int GetNeighbors(Point2D point, Span<(IRoomTile Tile, PathfinderEdge.EdgeDirection Direction)> buffer)
	{
		int i = 0;
		foreach ((Point2D location, PathfinderEdge.EdgeDirection direction) in RoomMap.directions)
		{
			Point2D newLocation = point + location;
			if (!this.IsValidLocation(newLocation))
			{
				continue;
			}

			IRoomTile tile = this.GetTile(newLocation);
			if (tile.IsHole)
			{
				continue;
			}

			buffer[i++] = (tile, direction);
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

	private readonly ref struct PathfinderData
	{
		private readonly PathfinderPoint[] pointsArray;
		private readonly Span2D<PathfinderPoint> points;

		internal PathfinderData(int height, int width)
		{
			PathfinderPoint[] pointsArray = ArrayPool<PathfinderPoint>.Shared.Rent(height * width);
			Array.Fill(pointsArray, new PathfinderPoint());

			this.pointsArray = pointsArray;
			this.points = new Span2D<PathfinderPoint>(pointsArray, height, width);
		}

		internal ref readonly PathfinderEdge this[int x, int y, double z]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref this.points[x, y][z];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref PathfinderEdge Get(int x, int y, double z) => ref this.points[x, y].Get(z);

		public void Dispose()
		{
			ArrayPool<PathfinderPoint>.Shared.Return(this.pointsArray);
		}

		[InlineArray(8)]
		internal struct NeighborsBuffer
		{
			private (IRoomTile Tile, PathfinderEdge.EdgeDirection Direction) data;
		}
	}

	private struct PathfinderPoint
	{
		private Dictionary<double, PathfinderEdge>? overflowLayers;

		private int index;
		private LayerEdgeIndexHolder indexes;
		private LayerEdgeHolder layers;

		public PathfinderPoint()
		{
			this.Indexes.Fill(double.NaN);
		}

		internal ref readonly PathfinderEdge this[double z]
		{
			[UnscopedRef]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (this.overflowLayers is null)
				{
					return ref this.layers[this.Indexes.IndexOf(z)];
				}

				return ref CollectionsMarshal.GetValueRefOrNullRef(this.overflowLayers, z);
			}
		}

		[UnscopedRef]
		internal ref PathfinderEdge Get(double z)
		{
			if (this.overflowLayers is null)
			{
				int index = this.Indexes.IndexOf(z);
				if (index != -1)
				{
					return ref this.layers[index];
				}

				if (this.index < this.Indexes.Length)
				{
					index = this.index++;

					this.indexes[index] = z;
					this.layers[index] = new PathfinderEdge();

					return ref this.layers[index];
				}
				else
				{
					Dictionary<double, PathfinderEdge> dictionary = new();
					for (int i = 0; i < this.Indexes.Length; i++)
					{
						dictionary[this.indexes[i]] = this.layers[i];
					}

					dictionary[z] = new PathfinderEdge();

					this.overflowLayers = dictionary;

					return ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, z);
				}
			}

			ref PathfinderEdge edge = ref CollectionsMarshal.GetValueRefOrAddDefault(this.overflowLayers, z, out bool exists);
			if (!exists)
			{
				edge = new PathfinderEdge();
			}

			return ref edge;
		}

		[InlineArray(4)]
		private struct LayerEdgeIndexHolder
		{
			private double layer;
		}

		[InlineArray(4)]
		private struct LayerEdgeHolder
		{
			private PathfinderEdge edge;
		}

		[UnscopedRef]
		internal Span<double> Indexes => this.indexes;
	}

	internal struct PathfinderEdge(int weight)
	{
		private int weight = weight;

		private EdgeDirection direction = EdgeDirection.None;
		private HeightsHolder heights;

		public PathfinderEdge()
			: this(int.MaxValue)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TrySuggestCandidate(int weight, EdgeDirection direction, double z)
		{
			if (weight > this.weight)
			{
				return false;
			}
			else if (weight == this.weight)
			{
				this.direction |= direction;
				this.heights[int.TrailingZeroCount((int)direction)] = z;

				return false;
			}

			this.weight = weight;
			this.direction = direction;
			this.heights[int.TrailingZeroCount((int)direction)] = z;

			return true;
		}

		internal readonly Point3D Backtrace(Point2D point)
		{
			return int.TrailingZeroCount((int)this.direction) switch
			{
				0 => new Point3D(point.X, point.Y - 1, this.heights[0]), //North
				1 => new Point3D(point.X - 1, point.Y, this.heights[1]), //East
				2 => new Point3D(point.X, point.Y + 1, this.heights[2]), //South
				3 => new Point3D(point.X + 1, point.Y, this.heights[3]), //West
				4 => new Point3D(point.X - 1, point.Y - 1, this.heights[4]), //NorthEast
				5 => new Point3D(point.X - 1, point.Y + 1, this.heights[5]), //SouthEast
				6 => new Point3D(point.X + 1, point.Y + 1, this.heights[6]), //SouthWest
				7 => new Point3D(point.X + 1, point.Y - 1, this.heights[7]), //NorthWest

				_ => throw new NotSupportedException()
			};
		}

		[Flags]
		internal enum EdgeDirection
		{
			None = 0,
			North = 1 << 0,
			East = 1 << 1,
			South = 1 << 2,
			West = 1 << 3,
			NorthEast = 1 << 4,
			SouthEast = 1 << 5,
			SouthWest = 1 << 6,
			NorthWest = 1 << 7,
		}

		[InlineArray(8)]
		private struct HeightsHolder
		{
			private double z;
		}
	}
}
