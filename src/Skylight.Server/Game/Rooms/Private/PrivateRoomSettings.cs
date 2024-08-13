using System.Collections.Immutable;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Private;

namespace Skylight.Server.Game.Rooms.Private;

internal sealed class PrivateRoomSettings(string name, string description, IServiceValue<INavigatorCategoryNode> category, ImmutableArray<string> tags, RoomEntryMode entryMode, int usersMax,
	RoomTradeMode tradeMode, bool walkThrough, bool allowPets, bool allowPetsToEat, IRoomCustomizationSettings customizationSettings, IRoomChatSettings chatSettings, IRoomModerationSettings moderationSettings) : IRoomSettings
{
	public string Name { get; } = name;
	public string Description { get; } = description;

	public IServiceValue<INavigatorCategoryNode> Category { get; } = category;
	public ImmutableArray<string> Tags { get; } = tags;

	public RoomEntryMode EntryMode { get; } = entryMode;
	public int UsersMax { get; } = usersMax;

	public RoomTradeMode TradeMode { get; } = tradeMode;
	public bool WalkThrough { get; } = walkThrough;
	public bool AllowPets { get; } = allowPets;
	public bool AllowPetsToEat { get; } = allowPetsToEat;

	public IRoomCustomizationSettings CustomizationSettings { get; } = customizationSettings;
	public IRoomChatSettings ChatSettings { get; } = chatSettings;
	public IRoomModerationSettings ModerationSettings { get; } = moderationSettings;
}
