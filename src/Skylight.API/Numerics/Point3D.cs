using System.Runtime.CompilerServices;

namespace Skylight.API.Numerics;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct Point3D(Point2D xy, double z) : IEquatable<Point3D>
{
	public Point2D XY { get; } = xy;
	public double Z { get; } = z;

	public int X => this.XY.X;
	public int Y => this.XY.Y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Point3D(int x, int y, double z)
		: this(new Point2D(x, y), z)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object? obj) => obj is Point3D other && this.Equals(other);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => HashCode.Combine(this.XY, this.Z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Point3D other) => other.XY.Equals(this.XY) && other.Z == this.Z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Point3D left, Point3D right)
	{
		return left.XY == right.XY && left.Z == right.Z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Point3D left, Point3D right)
	{
		return !(left == right);
	}
}
