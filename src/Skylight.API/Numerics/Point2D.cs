using System.Runtime.CompilerServices;

namespace Skylight.API.Numerics;

public struct Point2D : IEquatable<Point2D>
{
	public int X { get; set; }
	public int Y { get; set; }

	public Point2D(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Point2D other) => other == this;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DistanceSquared(Point2D left, Point2D right)
	{
		Point2D difference = left - right;

		return Point2D.Dot(difference, difference);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Dot(Point2D left, Point2D right)
	{
		return (left.X * right.X) + (left.Y * right.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Point2D operator +(Point2D left, Point2D right)
	{
		return new Point2D(left.X + right.X, left.Y + right.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Point2D operator -(Point2D left, Point2D right)
	{
		return new Point2D(left.X - right.X, left.Y - right.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Point2D left, Point2D right)
	{
		return left.X == right.X && left.Y == right.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Point2D left, Point2D right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj) => obj is Point2D other && this.Equals(other);
	public override int GetHashCode() => HashCode.Combine(this.X, this.Y);

	public override string ToString() => $"({this.X}, {this.Y})";
}
