using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Net.Communication.Attributes;
using Skylight.API.Game.Figure;
using Skylight.API.Game.Users;
using Skylight.Domain.Figure;
using Skylight.Domain.Users;
using Skylight.Infrastructure;
using Skylight.Protocol.Packets.Data.Avatar;
using Skylight.Protocol.Packets.Incoming.Avatar;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Avatar;
using Skylight.Server.Game.Figure;

namespace Skylight.Server.Game.Communication.Avatar;

[PacketManagerRegister(typeof(IGamePacketManager))]
internal sealed class GetWardrobePacketHandler<T>(IDbContextFactory<SkylightContext> dbContextFactory, IFigureConfigurationManager figureConfigurationManager) : UserPacketHandler<T>
	where T : IGetWardrobeIncomingPacket
{
	private readonly IDbContextFactory<SkylightContext> dbContextFactory = dbContextFactory;

	private readonly IFigureConfigurationManager figureConfigurationManager = figureConfigurationManager;

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(async client =>
		{
			await using SkylightContext dbContext = await this.dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

			IFigureConfigurationSnapshot figureConfigurationSnapshot = this.figureConfigurationManager.Current;

			List<WardrobeSlotData> wardrobe = dbContext.UserWardrobeSlots
				.Where(e => e.UserId == user.Profile.Id)
				.Include(e => e.FigureSets!)
				.ThenInclude(e => e.Colors)
				.AsEnumerable()
				.Select(s =>
				{
					Dictionary<IFigureSetType, FigureSetValue> figureSets = [];
					foreach (UserWardrobeSlotFigureEntity figureSetEntity in s.FigureSets!)
					{
						if (figureConfigurationSnapshot.TryGetFigureSet(figureSetEntity.SetId, out IFigureSet? set))
						{
							figureSets.Add(set.Type, new FigureSetValue(set, [.. figureSetEntity.Colors!.Select(id => set.Type.ColorPalette.Colors[id.ColorId])]));
						}
					}

					return new WardrobeSlotData(s.SlotId, s.Sex == FigureSexType.Male ? "M" : "F", new FigureDataContainer(figureSets.ToFrozenDictionary()).ToString());
				}).ToList();

			client.SendAsync(new WardrobeOutgoingPacket(wardrobe));
		});
	}
}
