namespace Skylight.API.Numerics;

public struct Point3D : IEquatable<Point3D>
{
	public Point2D XY { get; set; }
	public double Z { get; set; }

	public int X => this.XY.X;
	public int Y => this.XY.Y;

	public Point3D(int x, int y, double z)
		: this(new Point2D(x, y), z)
	{
	}

	public Point3D(Point2D xy, double z)
	{
		this.XY = xy;
		this.Z = z;
	}

	public override bool Equals(object? obj) => obj is Point3D other && this.Equals(other);
	public override int GetHashCode() => HashCode.Combine(this.XY, this.Z);

	public bool Equals(Point3D other) => other.XY.Equals(this.XY) && other.Z == this.Z;

	public static bool operator ==(Point3D left, Point3D right)
	{
		return left.XY == right.XY && left.Z == right.Z;
	}

	public static bool operator !=(Point3D left, Point3D right)
	{
		return !(left == right);
	}
}
