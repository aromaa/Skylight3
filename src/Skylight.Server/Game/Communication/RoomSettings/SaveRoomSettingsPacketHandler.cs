using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Private;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Room.Engine;
using Skylight.Protocol.Packets.Incoming.RoomSettings;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Navigator;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Protocol.Packets.Outgoing.RoomSettings;
using Skylight.Server.Game.Rooms.Private;

namespace Skylight.Server.Game.Communication.RoomSettings;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class SaveRoomSettingsPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, INavigatorManager navigatorManager, IRoomManager roomManager) : UserPacketHandler<T>
	where T : ISaveRoomSettingsIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly INavigatorManager navigatorManager = navigatorManager;
	private readonly IRoomManager roomManager = roomManager;

	internal override void Handle(IUser user, in T packet)
	{
		if (!this.navigatorManager.TryGetNode(packet.CategoryId, out IServiceValue<INavigatorCategoryNode>? category))
		{
			return;
		}

		if (packet.Tags.Count > 2)
		{
			return;
		}

		int roomId = packet.RoomId;

		string name = user.Client.Encoding.GetString(packet.Name);
		string description = user.Client.Encoding.GetString(packet.Description);

		ImmutableArray<string>.Builder tagsBuilder = ImmutableArray.CreateBuilder<string>(2);
		foreach (ReadOnlySequence<byte> tag in packet.Tags)
		{
			tagsBuilder.Add(user.Client.Encoding.GetString(tag));
		}

		ImmutableArray<string> tags = tagsBuilder.ToImmutable();

		RoomEntryMode entryMode = packet.EntryMode switch
		{
			RoomEntryType.Open => RoomEntryMode.Open(),
			RoomEntryType.Locked => RoomEntryMode.Locked(),
			RoomEntryType.Password => RoomEntryMode.Password(user.Client.Encoding.GetString(packet.Password)),
			RoomEntryType.Invisible => RoomEntryMode.Invisible(),

			_ => throw new NotSupportedException()
		};

		int usersMax = packet.UsersMax;

		RoomTradeMode tradeMode = packet.TradeMode switch
		{
			RoomTradeType.None => RoomTradeMode.None,
			RoomTradeType.WithRights => RoomTradeMode.WithRights,
			RoomTradeType.Everyone => RoomTradeMode.Everyone,

			_ => throw new NotSupportedException()
		};

		bool walkThrough = packet.WalkThrough;
		bool allowPets = packet.AllowPets;
		bool allowPetsToEat = packet.AllowPetsToEat;

		PrivateRoomCustomizationSettings customizationSettings = new(packet.HideWalls, packet.FloorThickness, packet.WallThickness);

		user.Client.ScheduleTask(async client =>
		{
			IPrivateRoomInfo? roomInfo = await this.navigatorManager.GetPrivateRoomInfoAsync(roomId).ConfigureAwait(false);
			if (roomInfo is null)
			{
				client.SendAsync(new NoSuchFlatOutgoingPacket(roomId));

				return;
			}

			if (roomInfo.Owner.Id != user.Profile.Id)
			{
				return;
			}

			await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
			{
				PrivateRoomEntryMode dbEntryMode = entryMode.Type switch
				{
					RoomEntryMode.ModeType.Open => PrivateRoomEntryMode.Open,
					RoomEntryMode.ModeType.Locked => PrivateRoomEntryMode.Locked,
					RoomEntryMode.ModeType.Password => PrivateRoomEntryMode.Password,
					RoomEntryMode.ModeType.Invisible => PrivateRoomEntryMode.Invisible,
					RoomEntryMode.ModeType.NoobsOnly => PrivateRoomEntryMode.NoobsOnly,

					_ => throw new NotSupportedException()
				};

				PrivateRoomTradeMode dbTradeMode = tradeMode switch
				{
					RoomTradeMode.None => PrivateRoomTradeMode.None,
					RoomTradeMode.WithRights => PrivateRoomTradeMode.WithRights,
					RoomTradeMode.Everyone => PrivateRoomTradeMode.Everyone,

					_ => throw new NotSupportedException()
				};

				await dbContext.PrivateRooms
					.Where(e => e.Id == roomId)
					.ExecuteUpdateAsync(setters => setters
						.SetProperty(e => e.Name, name)
						.SetProperty(e => e.Description, description)
						.SetProperty(e => e.CategoryId, category.Value.Id)
						.SetProperty(e => e.Tags, ImmutableCollectionsMarshal.AsArray(tags)!)
						.SetProperty(e => e.EntryMode, dbEntryMode)
						.SetProperty(e => e.Password, entryMode.Type == RoomEntryMode.ModeType.Password ? ((RoomEntryMode.PasswordProtected)entryMode).Password : null)
						.SetProperty(e => e.UsersMax, usersMax)
						.SetProperty(e => e.TradeMode, dbTradeMode)
						.SetProperty(e => e.AllowPets, allowPets)
						.SetProperty(e => e.AllowPetsToEat, allowPetsToEat)
						.SetProperty(e => e.HideWalls, customizationSettings.HideWalls)
						.SetProperty(e => e.FloorThickness, customizationSettings.FloorThickness)
						.SetProperty(e => e.WallThickness, customizationSettings.WallThickness))
					.ConfigureAwait(false);
			}

			if (!this.roomManager.TryGetPrivateRoom(roomId, out IPrivateRoom? room))
			{
				return;
			}

			roomInfo.Settings = new PrivateRoomSettings(name, description, category, tags, entryMode, usersMax, tradeMode, walkThrough, allowPets, allowPetsToEat, customizationSettings, new PrivateRoomChatSettings(), new PrivateRoomModerationSettings());

			client.SendAsync(new RoomSettingsSavedOutgoingPacket(roomId));

			room.SendAsync(new RoomInfoUpdatedOutgoingPacket(roomId));
			room.SendAsync(new RoomVisualizationSettingsOutgoingPacket(customizationSettings.HideWalls, customizationSettings.FloorThickness, customizationSettings.WallThickness));
		});
	}
}
