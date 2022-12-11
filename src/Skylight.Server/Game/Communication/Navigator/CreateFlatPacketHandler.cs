using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
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
internal sealed class CreateFlatPacketHandler<T> : UserPacketHandler<T>
	where T : ICreateFlatIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	private readonly INavigatorManager navigatorManager;

	public CreateFlatPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager)
	{
		this.dbContextFactory = dbContextFactory;

		this.navigatorManager = navigatorManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		ReadOnlySpan<byte> roomName = packet.RoomName.ToArray();

		int roomNameCharCount = Encoding.UTF8.GetCharCount(roomName);
		if (roomNameCharCount is < 3 or > 25)
		{
			return;
		}

		ReadOnlySpan<byte> description = packet.Description.ToArray();

		int descriptionCharCount = Encoding.UTF8.GetCharCount(description);
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

		user.Client.ScheduleTask(new CreateFlatTask
		{
			DbContextFactory = this.dbContextFactory,

			RoomName = Encoding.UTF8.GetString(packet.RoomName),
			Description = Encoding.UTF8.GetString(packet.Description),
			Layout = layout,

			FlatCat = flatCat,

			MaxUserCount = packet.MaxUserCount
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct CreateFlatTask : IClientTask
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal string RoomName { get; init; }
		internal string Description { get; init; }
		internal IRoomLayout Layout { get; init; }

		internal IRoomFlatCat FlatCat { get; init; }

		internal int MaxUserCount { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			RoomEntity room;
			await using (SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
			{
				dbContext.Rooms.Add(room = new RoomEntity
				{
					Name = this.RoomName,
					Description = this.Description,
					LayoutId = this.Layout.Id,

					CategoryId = this.FlatCat.Id,

					UsersMax = this.MaxUserCount,

					OwnerId = client.User!.Profile.Id
				});

				await dbContext.SaveChangesAsync().ConfigureAwait(false);
			}

			client.SendAsync(new FlatCreatedOutgoingPacket(room.Id, room.Name));
		}
	}
}
