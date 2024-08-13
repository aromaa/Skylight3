using Skylight.API.Game.Rooms.Private;
using Skylight.Protocol.Packets.Data.Room.Engine;

namespace Skylight.Server.Extensions;

internal static class RoomTradeModeExtensions
{
	internal static RoomTradeType ToProtocol(this RoomTradeMode mode) => mode switch
	{
		RoomTradeMode.None => RoomTradeType.None,
		RoomTradeMode.WithRights => RoomTradeType.WithRights,
		RoomTradeMode.Everyone => RoomTradeType.Everyone,

		_ => throw new NotSupportedException()
	};
}
