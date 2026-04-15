using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Users;
using Skylight.API.Game.Users.Rooms;
using Skylight.Protocol.Packets.Incoming.Room.Session;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Room.Session;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GoToFlatPacketHandler<T> : ClientPacketHandler<T>
	where T : IGoToFlatIncomingPacket
{
	internal override void Handle(IClient client, in T packet)
	{
		int roomId = packet.RoomId;

		if (client.User is { } user)
		{
			this.OpenSession(user, roomId);
		}
		else
		{
			//TODO: Temp fix
			client.ScheduleTask(client =>
			{
				while (!client.Socket.Closed)
				{
					if (client.User is { } user)
					{
						this.OpenSession(user, roomId);
						break;
					}

					Thread.Sleep(100);
				}

				return Task.CompletedTask;
			});
		}
	}

	private void OpenSession(IUser user, int roomId)
	{
		IRoomSession session = user.GetOrOpenRoomSession(0, roomId, out bool opened);

		user.Client.ScheduleTask(async _ =>
		{
			if (opened)
			{
				await session.OpenRoomAsync().ConfigureAwait(false);
			}

			session.TryEnterRoom();
		});
	}
}
