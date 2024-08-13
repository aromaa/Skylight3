using System.Collections.Immutable;
using Skylight.API.DependencyInjection;
using Skylight.API.Game.Navigator.Nodes;

namespace Skylight.API.Game.Rooms.Private;

public interface IRoomSettings
{
	public string Name { get; }
	public string Description { get; }

	public IServiceValue<INavigatorCategoryNode> Category { get; }
	public ImmutableArray<string> Tags { get; }

	public RoomEntryMode EntryMode { get; }
	public int UsersMax { get; }

	public RoomTradeMode TradeMode { get; }
	public bool WalkThrough { get; }
	public bool AllowPets { get; }
	public bool AllowPetsToEat { get; }

	public IRoomCustomizationSettings CustomizationSettings { get; }
	public IRoomChatSettings ChatSettings { get; }
	public IRoomModerationSettings ModerationSettings { get; }
}
