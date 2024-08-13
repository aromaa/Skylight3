using Skylight.API.Game.Rooms.Private;
using Skylight.Protocol.Packets.Data.Room.Engine;

namespace Skylight.Server.Extensions;

internal static class RoomEntryModeExtensions
{
	internal static RoomEntryType ToProtocol(this RoomEntryMode mode) => mode.Type switch
	{
		RoomEntryMode.ModeType.Open => RoomEntryType.Open,
		RoomEntryMode.ModeType.Locked => RoomEntryType.Locked,
		RoomEntryMode.ModeType.Password => RoomEntryType.Password,
		RoomEntryMode.ModeType.Invisible => RoomEntryType.Invisible,
		RoomEntryMode.ModeType.NoobsOnly => RoomEntryType.NoobsOnly,

		_ => throw new NotSupportedException()
	};
}
