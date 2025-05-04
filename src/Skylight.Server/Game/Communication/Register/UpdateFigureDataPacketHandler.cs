using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Register;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;

namespace Skylight.Server.Game.Communication.Register;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class UpdateFigureDataPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : IUpdateFigureDataIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		string figure = Encoding.ASCII.GetString(packet.Figure);
		string gender = Encoding.ASCII.GetString(packet.Gender);

		user.Client.ScheduleTask(async client =>
		{
			user.Profile.Figure = figure;
			user.Profile.Gender = gender;

			await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
			{
				await dbContext.Users
					.Where(u => u.Id == user.Profile.Id)
					.ExecuteUpdateAsync(setters => setters
						.SetProperty(u => u.Gender, gender)
						.SetProperty(u => u.Figure, figure)).ConfigureAwait(false);
			}

			if (user.RoomSession?.Unit is { } roomUnit)
			{
				roomUnit.Room.SendAsync(new UserChangeOutgoingPacket(roomUnit.Id, user.Profile.Figure, user.Profile.Gender, user.Profile.Motto, 666));
			}

			user.SendAsync(new UserChangeOutgoingPacket(-1, user.Profile.Figure, user.Profile.Gender, user.Profile.Motto, 666));
		});
	}
}
