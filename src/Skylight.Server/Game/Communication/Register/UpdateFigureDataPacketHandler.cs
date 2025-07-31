using System.Text;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Net.Communication.Attributes;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Register;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Room.Engine;
using Skylight.Server.Extensions;

namespace Skylight.Server.Game.Communication.Register;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class UpdateFigureDataPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, IFigureConfigurationManager figureConfigurationManager) : UserPacketHandler<T>
	where T : IUpdateFigureDataIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFigureConfigurationManager figureConfigurationManager = figureConfigurationManager;

	internal override void Handle(IUser user, in T packet)
	{
		FigureSex sex = Encoding.ASCII.GetString(packet.Gender) == "M" ? FigureSex.Male : FigureSex.Female;

		this.figureConfigurationManager.TryGetFigureValidator("user", sex, out IFigureValidator? validator);

		IFigureDataContainer figure = this.figureConfigurationManager.Parse(packet.Figure, new FigureValidationOptions(validator, user.PermissionSubject));

		user.Client.ScheduleTask(async client =>
		{
			user.Profile.Avatar = new FigureAvatar(sex, figure);

			// TODO: Retry on deadlock?
			await using (SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
			{
				await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

				await dbContext.UserFigureSets
					.ToLinqToDBTable()
					.Merge()
					.Using(figure.Sets.Select(e => (SetTypeId: e.Key.Id, SetId: e.Value.Set.Id)))
					.On((e, f) => e.UserId == user.Profile.Id && e.SetTypeId == f.SetTypeId)
					.UpdateWhenMatched(static (e, f) => new UserFigureEntity
					{
						SetId = f.SetId
					})
					.InsertWhenNotMatched(f => new UserFigureEntity
					{
						UserId = user.Profile.Id,
						SetTypeId = f.SetTypeId,
						SetId = f.SetId
					})
					.DeleteWhenNotMatchedBySourceAnd(e => e.UserId == user.Profile.Id)
					.MergeAsync()
					.ConfigureAwait(false);

				await dbContext.UserFigureColors
					.ToLinqToDBTable()
					.Merge()
					.Using(figure.Sets.SelectMany(static e => e.Value.Colors.Select(static (v, i) => (Index: i, Value: v)),
						(p, v) => (SetTypeId: p.Key.Id, Index: v.Index, ColorId: v.Value.Id)))
					.On((e, f) => e.UserId == user.Profile.Id && e.SetTypeId == f.SetTypeId && e.Index == f.Index)
					.UpdateWhenMatched(static (e, f) => new UserFigureColorEntity
					{
						ColorId = f.ColorId
					})
					.InsertWhenNotMatched(f => new UserFigureColorEntity
					{
						UserId = user.Profile.Id,
						SetTypeId = f.SetTypeId,
						Index = f.Index,
						ColorId = f.ColorId
					})
					.DeleteWhenNotMatchedBySourceAnd(e => e.UserId == user.Profile.Id)
					.MergeAsync()
					.ConfigureAwait(false);

				await transaction.CommitAsync().ConfigureAwait(false);
			}

			if (user.RoomSession?.Unit is { } roomUnit)
			{
				roomUnit.Room.SendAsync(new UserChangeOutgoingPacket(roomUnit.Id, user.Profile.Avatar.Data.ToString(), user.Profile.Avatar.Sex.ToNetwork(), user.Profile.Motto, 666));
			}

			user.SendAsync(new UserChangeOutgoingPacket(-1, user.Profile.Avatar.Data.ToString(), user.Profile.Avatar.Sex.ToNetwork(), user.Profile.Motto, 666));
		});
	}
}
