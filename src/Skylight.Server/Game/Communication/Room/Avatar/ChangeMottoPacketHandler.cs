using System.Text;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Room.Avatar;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Room.Avatar;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed partial class ChangeMottoPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory) : UserPacketHandler<T>
	where T : IChangeMottoIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	internal override void Handle(IUser user, in T packet)
	{
		if (user.RoomSession?.Unit is not { } roomUnit)
		{
			return;
		}

		string motto = user.Client.Encoding.GetString(packet.Motto);
		if (motto.Length > 38 || motto == user.Profile.Motto)
		{
			return;
		}

		user.Client.ScheduleTask(async client =>
		{
			client.User!.Profile.Motto = motto;

			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			UserEntity entity = new()
			{
				Id = client.User.Profile.Id
			};

			dbContext.Users.Attach(entity);

			entity.Motto = motto;

			await dbContext.SaveChangesAsync().ConfigureAwait(false);

			roomUnit.Room.SendAsync(new UserChangeOutgoingPacket(client.User.Profile.Id, client.User.Profile.Avatar.Data.ToString(), client.User.Profile.Avatar.Sex.ToNetwork(), client.User.Profile.Motto, 666));
		});
	}
}
