namespace Skylight.API.Game.Rooms.Private;

public readonly struct RoomEntryMode
{
	public ModeType Type { get; }

	private readonly object? value;

	private RoomEntryMode(ModeType type, object? value = null)
	{
		this.Type = type;
		this.value = value;
	}

	public static RoomEntryMode Open() => new(ModeType.Open);
	public static RoomEntryMode Locked() => new(ModeType.Locked);
	public static RoomEntryMode Password(string password) => new(ModeType.Password, password);
	public static RoomEntryMode Invisible() => new(ModeType.Invisible);
	public static RoomEntryMode NoobsOnly() => new(ModeType.NoobsOnly);

	public static explicit operator PasswordProtected(RoomEntryMode union)
	{
		if (union.Type == ModeType.Password)
		{
			return new PasswordProtected(union.Type, (string)union.value!);
		}

		throw new ArgumentException("Type was not password", nameof(union));
	}

	public readonly struct PasswordProtected(ModeType type, string password)
	{
		public ModeType Type { get; } = type;

		public string Password { get; } = password;
	}

	public enum ModeType
	{
		Open,
		Locked,
		Password,
		Invisible,
		NoobsOnly
	}
}
