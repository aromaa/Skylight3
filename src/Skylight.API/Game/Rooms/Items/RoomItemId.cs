namespace Skylight.API.Game.Rooms.Items;

public readonly struct RoomItemId(IRoomItemDomain domain, int id) : IEquatable<RoomItemId>
{
	public IRoomItemDomain Domain { get; } = domain;
	public int Id { get; } = id;

	public bool Equals(RoomItemId other) => this.Domain == other.Domain && this.Id == other.Id;

	public override bool Equals(object? obj) => obj is RoomItemId other && this.Equals(other);
	public override int GetHashCode() => HashCode.Combine(this.Domain, this.Id);

	public static bool operator ==(RoomItemId left, RoomItemId right) => left.Equals(right);
	public static bool operator !=(RoomItemId left, RoomItemId right) => !left.Equals(right);
}
