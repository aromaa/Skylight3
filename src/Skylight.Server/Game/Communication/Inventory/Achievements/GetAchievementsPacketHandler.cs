using Net.Communication.Attributes;
using Skylight.API.Game.Achievements;
using Skylight.API.Game.Users;
using Skylight.Protocol.Packets.Data.Inventory.Achievements;
using Skylight.Protocol.Packets.Incoming.Inventory.Achievements;
using Skylight.Protocol.Packets.Manager;
using Skylight.Protocol.Packets.Outgoing.Inventory.Achievements;

namespace Skylight.Server.Game.Communication.Inventory.Achievements;

[PacketManagerRegister(typeof(AbstractGamePacketManager))]
internal sealed class GetAchievementsPacketHandler<T> : UserPacketHandler<T>
	where T : IGetAchievementsIncomingPacket
{
	private readonly IAchievementManager achievementManager;

	public GetAchievementsPacketHandler(IAchievementManager achievementManager)
	{
		this.achievementManager = achievementManager;
	}

	internal override void Handle(IUser user, in T packet)
	{
		List<AchievementData> achievements = new();

		foreach (IAchievement achievement in this.achievementManager.Achievements)
		{
			//State definitions - 0 hidden, 1 enabled, 2 archived, 3 seasonal
			if (achievement.State == 0)
			{
				continue;
			}

			//Level 0: currentLevel = null and nextLevel = achievement.Levels[0]
			//Level 1: currentLevel = non null and nextLevel = currentLevel.NextLevel
			//Level max: currentLevel = non null and nextLevel = currentLevel
			IAchievementLevel? currentLevel = null;
			IAchievementLevel nextLevel = currentLevel is not null ? (currentLevel.NextLevel ?? currentLevel) : achievement.Levels[0];

			achievements.Add(new AchievementData
			{
				Id = achievement.Id,

				Category = achievement.Category,

				NextLevel = nextLevel.Level,
				NextLevelBadgeCode = nextLevel.Badge.Code,

				MaximumLevel = achievement.Levels.Length,
				Completed = currentLevel == nextLevel,

				CurrentProgress = 0,
				PreviousProgressRequirement = currentLevel?.ProgressRequirement ?? 0,
				CurrentProgressRequirement = nextLevel.ProgressRequirement,

				NextLevelRewardPoints = 0,
				NextLevelRewardPointsType = 0,

				DisplayMode = achievement.DisplayProgress ? 0 : 1,
				State = achievement.State
			});
		}

		user.SendAsync(new AchievementsOutgoingPacket(achievements, string.Empty));
	}
}
