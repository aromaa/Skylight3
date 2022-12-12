using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Inventory.Items.Floor;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Sound;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Sound;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class EditSongPacketHandler<T> : UserPacketHandler<T>
	where T : IEditSongIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public EditSongPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		user.Client.ScheduleTask(new EditSongTask
		{
			DbContextFactory = this.dbContextFactory,

			Unit = unit,

			SongId = packet.SongId
		});
	}

	[StructLayout(LayoutKind.Auto)]
	private readonly struct EditSongTask : IClientTask, IRoomTask
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal int SongId { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			int songId = this.SongId;

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			SongEntity? songEntity = await dbContext.Songs.FirstOrDefaultAsync(s => s.Id == songId).ConfigureAwait(false);
			if (songEntity is null)
			{
				return;
			}

			this.Unit.User.SendAsync(new SongInfoOutgoingPacket(songEntity.Id, songEntity.Name, songEntity.Data));

			this.Unit.Room.ScheduleTask(this);
		}

		public void Execute(IRoom room)
		{
			if (!this.Unit.InRoom || !this.Unit.Room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
			{
				return;
			}

			List<SoundSetData> filledSlots = new();
			foreach ((int slot, ISoundSetFurniture soundSet) in soundMachine.SoundSets)
			{
				filledSlots.Add(new SoundSetData(slot, soundSet.SoundSetId, soundSet.Samples));
			}

			this.Unit.User.SendAsync(new TraxSoundPackagesOutgoingPacket(soundMachine.Furniture.SoundSetSlotCount, filledSlots));

			List<int> soundSets = new();
			foreach (IFloorInventoryItem item in this.Unit.User.Inventory.FloorItems)
			{
				if (item is not ISoundSetInventoryItem soundSet)
				{
					continue;
				}

				if (soundMachine.HasSoundSet(soundSet.Furniture.SoundSetId))
				{
					continue;
				}

				soundSets.Add(soundSet.Furniture.SoundSetId);
			}

			this.Unit.User.SendAsync(new UserSoundPackagesOutgoingPacket(soundSets));
		}
	}
}
