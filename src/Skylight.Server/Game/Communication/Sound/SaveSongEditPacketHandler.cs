using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Clients;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Units;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Sound;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class SaveSongEditPacketHandler<T> : UserPacketHandler<T>
	where T : ISaveSongEditIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory;

	public SaveSongEditPacketHandler(IDbContextFactory<SkylightContext> dbContextFactory)
	{
		this.dbContextFactory = dbContextFactory;
	}

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } unit)
		{
			return;
		}

		string name = Encoding.UTF8.GetString(packet.Name);
		string songData = Encoding.UTF8.GetString(packet.SongData);

		int songLength = 0;
		foreach (Match match in SaveSongEditPacketHandler<T>.ParseSongData().Matches(songData))
		{
			if (match.Length == 0)
			{
				continue;
			}

			int channel = int.Parse(match.Groups[1].ValueSpan);

			int channelLength = 0;
			foreach (Capture capture in match.Groups[2].Captures)
			{
				ReadOnlySpan<char> value = capture.ValueSpan;

				int separator = value.IndexOf(',');

				int sampleId = int.Parse(value[..separator]);
				int length = int.Parse(value.Slice(separator + 1));

				channelLength += length;
			}

			if (channelLength > songLength)
			{
				songLength = channelLength;
			}
		}

		user.Client.ScheduleTask(new SaveSongTask
		{
			DbContextFactory = this.dbContextFactory,

			Unit = unit,

			SongId = packet.SongId,

			Name = name,
			Length = songLength,
			Data = songData
		});
	}

	[GeneratedRegex("(?:([1-4])(?::(?:([0-9]+,[0-9]+);?)+:))?")]
	private static partial Regex ParseSongData();

	[StructLayout(LayoutKind.Auto)]
	private readonly struct SaveSongTask : IClientTask
	{
		internal IDbContextFactory<SkylightContext> DbContextFactory { get; init; }

		internal IUserRoomUnit Unit { get; init; }

		internal int SongId { get; init; }

		internal string Name { get; init; }
		internal int Length { get; init; }
		internal string Data { get; init; }

		public async Task ExecuteAsync(IClient client)
		{
			int soundMachineId = await this.Unit.Room.ScheduleTaskAsync(static (room, roomUnit) =>
			{
				if (!roomUnit.InRoom || !room.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
				{
					return 0;
				}

				return soundMachine.Id;
			}, this.Unit).ConfigureAwait(false);

			if (soundMachineId == 0)
			{
				return;
			}

			await using SkylightContext dbContext = await this.DbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			SongEntity songEntity = new()
			{
				Id = this.SongId
			};

			dbContext.Attach(songEntity);

			songEntity.Name = this.Name;
			songEntity.Length = this.Length;
			songEntity.Data = this.Data;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);

			client.SendAsync(new NewSongOutgoingPacket(songEntity.Id, this.Name));
		}
	}
}
