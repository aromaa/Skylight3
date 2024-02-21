using System.Buffers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Map;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Navigator;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;

namespace Skylight.Server.Game.Communication.Navigator;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class CreateFlatPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	: UserPacketHandler<T>
	where T : ICreateFlatIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;

	internal override void Handle(IUser user, in T packet)
	{
		ReadOnlySpan<byte> roomNameSpan = packet.RoomName.ToArray();

		int roomNameCharCount = Encoding.UTF8.GetCharCount(roomNameSpan);
		if (roomNameCharCount is < 3 or > 25)
		{
			return;
		}

		ReadOnlySpan<byte> descriptionSpan = packet.Description.ToArray();

		int descriptionCharCount = Encoding.UTF8.GetCharCount(descriptionSpan);
		if (descriptionCharCount > 128)
		{
			return;
		}

		string model = Encoding.UTF8.GetString(packet.LayoutId);
		if (!this.navigatorManager.TryGetLayout(model, out IRoomLayout? layout))
		{
			return;
		}

		if (!this.navigatorManager.TryGetFlatCat(packet.CategoryId, out IRoomFlatCat? flatCat))
		{
			return;
		}

		if (packet.MaxUserCount % 5 != 0 || packet.MaxUserCount is < 10 or > 75)
		{
			return;
		}

		string roomName = Encoding.UTF8.GetString(packet.RoomName);
		string description = Encoding.UTF8.GetString(packet.Description);

		int maxUserCount = packet.MaxUserCount;

		user.Client.ScheduleTask(async client =>
		{
			RoomEntity room;
			await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
			{
				dbContext.Rooms.Add(room = new RoomEntity
				{
					Name = roomName,
					Description = description,
					LayoutId = layout.Id,

					CategoryId = flatCat.Id,

					UsersMax = maxUserCount,

					OwnerId = client.User!.Profile.Id
				});

				await dbContext.SaveChangesAsync().ConfigureAwait(false);
			}

			client.SendAsync(new FlatCreatedOutgoingPacket(room.Id, room.Name));
		});
	}
}
