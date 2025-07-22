using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items;
using Skylight.Protocol.Packets.Convertors.Room.Engine;
using Skylight.Protocol.Packets.Manager;
using Skylight.Server.Game.Rooms.Items.Domains;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class RoomItemIdConverter : IRoomItemIdConverter<RoomItemId>
{
	public static int Convert(RoomItemId value)
	{
		return value.Domain switch
		{
			NormalRoomItemDomain => value.Id,
			BuildersClubRoomItemDomain => int.MaxValue - value.Id,
			TransientRoomItemDomain => 0x7FFF0000 + value.Id,

			_ => throw new NotSupportedException($"Unsupported domain type: {value.Domain.GetType()}")
		};
	}
}
