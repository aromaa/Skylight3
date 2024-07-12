using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Private;
using Skylight.API.Game.Users;
using Skylight.Domain.Rooms.Sound;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Sound;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Sound;

namespace Skylight.Server.Game.Communication.Sound;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class SaveSongEditPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : ISaveSongEditIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { Room: IPrivateRoom privateRoom } roomUnit || !privateRoom.IsOwner(user))
		{
			return;
		}

		string name = user.Client.Encoding.GetString(packet.Name);
		string songData = Encoding.ASCII.GetString(packet.SongData);

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

		int songId = packet.SongId;

		user.Client.ScheduleTask(async client =>
		{
			int soundMachineId = await privateRoom.ScheduleTask(_ =>
			{
				if (!roomUnit.InRoom || !privateRoom.ItemManager.TryGetInteractionHandler(out ISoundMachineInteractionManager? handler) || handler.SoundMachine is not { } soundMachine)
				{
					return 0;
				}

				return soundMachine.Id;
			}).ConfigureAwait(false);

			if (soundMachineId == 0)
			{
				return;
			}

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			SongEntity songEntity = new()
			{
				Id = songId
			};

			dbContext.Attach(songEntity);

			songEntity.Name = name;
			songEntity.Length = songLength;
			songEntity.Data = songData;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);

			client.SendAsync(new NewSongOutgoingPacket(songEntity.Id, name));
		});
	}

	[GeneratedRegex("(?:([1-4])(?::(?:([0-9]+,[0-9]+);?)+:))?")]
	private static partial Regex ParseSongData();
}
