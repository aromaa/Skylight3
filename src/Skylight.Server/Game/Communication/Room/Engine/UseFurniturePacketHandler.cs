using System.Runtime.InteropServices;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Floor;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Incoming.Room.Engine;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Engine;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class UseFurniturePacketHandler<T> : UserPacketHandler<T>
	where T : IUseFurnitureIncomingPacket
{
	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		roomUnit.Room.ScheduleTask(new UseFurnitureTask
		{
			User = user,

			Id = packet.Id,
			State = packet.State
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct UseFurnitureTask : IRoomTask
	{
		public IUser User { get; init; }

		public int Id { get; init; }
		public int State { get; init; }

		public void Execute(IRoom room)
		{
			if (!room.ItemManager.TryGetFloorItem(this.Id, out IFloorRoomItem? item))
			{
				return;
			}
		}
	}
}
