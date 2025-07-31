using System.Text;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Net.Communication.Attributes;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Figure;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Incoming.Avatar;
using Skylight.Protocol.Packets.Manager;

namespace Skylight.Server.Game.Communication.Avatar;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class SaveWardrobeOutfitPacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, IFigureConfigurationManager figureConfigurationManager) : UserPacketHandler<T>
	where T : ISaveWardrobeOutfitIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFigureConfigurationManager figureConfigurationManager = figureConfigurationManager;

	internal override void Handle(IUser user, in T packet)
	{
		int slotId = packet.SlotId;
		if (slotId is <= 0 or > 10)
		{
			return;
		}

		IFigureDataContainer figure = this.figureConfigurationManager.Parse(packet.Figure, new FigureValidationOptions(null, user.PermissionSubject));
		FigureSexType sex = Encoding.ASCII.GetString(packet.Gender) == "M" ? FigureSexType.Male : FigureSexType.Female;

		user.Client.ScheduleTask(async _ =>
		{
			// TODO: Retry on deadlock?
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
			await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

			await dbContext.Upsert(new UserWardrobeSlotEntity
			{
				UserId = user.Profile.Id,
				SlotId = slotId,
				Sex = sex
			}).WhenMatched((_, u) => new UserWardrobeSlotEntity
			{
				Sex = u.Sex
			}).RunAsync().ConfigureAwait(false);

			await dbContext.UserWardrobeSlotFigure
				.ToLinqToDBTable()
				.Merge()
				.Using(figure.Sets.Select(e => (UserId: user.Profile.Id, SetTypeId: e.Key.Id, SetId: e.Value.Set.Id)))
				.On((e, f) => e.UserId == user.Profile.Id && e.SlotId == slotId && e.SetTypeId == f.SetTypeId)
				.UpdateWhenMatched(static (e, f) => new UserWardrobeSlotFigureEntity
				{
					SetId = f.SetId
				})
				.InsertWhenNotMatched(f => new UserWardrobeSlotFigureEntity
				{
					UserId = f.UserId,
					SlotId = slotId,
					SetTypeId = f.SetTypeId,
					SetId = f.SetId
				})
				.DeleteWhenNotMatchedBySourceAnd(e => e.UserId == user.Profile.Id && e.SlotId == slotId)
				.MergeAsync()
				.ConfigureAwait(false);

			await dbContext.UserWardrobeSlotFigureColors
				.ToLinqToDBTable()
				.Merge()
				.Using(figure.Sets.SelectMany(static e => e.Value.Colors.Select(static (v, i) => (Index: i, Value: v)),
					(p, v) => (UserId: user.Profile.Id, SetTypeId: p.Key.Id, Index: v.Index, ColorId: v.Value.Id)))
				.On((e, f) => e.UserId == user.Profile.Id && e.SlotId == slotId && e.SetTypeId == f.SetTypeId && e.Index == f.Index)
				.UpdateWhenMatched(static (e, f) => new UserWardrobeSlotFigureColorEntity
				{
					ColorId = f.ColorId
				})
				.InsertWhenNotMatched(f => new UserWardrobeSlotFigureColorEntity
				{
					UserId = f.UserId,
					SlotId = slotId,
					SetTypeId = f.SetTypeId,
					Index = f.Index,
					ColorId = f.ColorId
				})
				.DeleteWhenNotMatchedBySourceAnd(e => e.UserId == user.Profile.Id && e.SlotId == slotId)
				.MergeAsync()
				.ConfigureAwait(false);

			await transaction.CommitAsync().ConfigureAwait(false);
		});
	}
}
