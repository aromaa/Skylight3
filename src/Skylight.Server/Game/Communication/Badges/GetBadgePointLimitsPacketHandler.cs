using System.Text.RegularExpressions;
using Net.Communication.Attributes;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Inventory.Badges;
using Skylight.Protocol.Packets.Incoming.Badges;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Badges;

namespace Skylight.Server.Game.Communication.Badges;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed partial class GetBadgePointLimitsPacketHandler<T>(IAchievementManager achievementManager) : UserPacketHandler<T>
	where T : IGetBadgePointLimitsIncomingPacket
{
	private readonly IAchievementManager achievementManager = achievementManager;

	internal override void Handle(IUser user, in T packet)
	{
		user.Client.ScheduleTask(async client =>
		{
			IAchievementSnapshot snapshot = await this.achievementManager.GetAsync().ConfigureAwait(false);

			Dictionary<string, BadgePointLimitData> badgePointLimits = [];
			foreach ((string badgeCode, int limit) in snapshot.BadgePointLimits)
			{
				Match match = GetBadgePointLimitsPacketHandler<T>.ParseAchievementBadge().Match(badgeCode);

				string badgeGroup = match.Groups[1].Value;
				if (!badgePointLimits.TryGetValue(badgeGroup, out BadgePointLimitData? badgePointLimit))
				{
					badgePointLimit = badgePointLimits[badgeGroup] = new BadgePointLimitData(badgeGroup, []);
				}

				((List<(int Level, int Limit)>)badgePointLimit.Limits).Add((int.Parse(match.Groups[2].ValueSpan), limit));
			}

			client.SendAsync(new BadgePointLimitsOutgoingPacket(badgePointLimits.Values));
		});
	}

	[GeneratedRegex("ACH_(.+?)([0-9]+)$")]
	private static partial Regex ParseAchievementBadge();
}
