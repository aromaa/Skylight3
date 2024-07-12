using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Rooms.Private;

namespace Skylight.API.Game.Rooms.Items.Interactions;

public interface IRoomItemInteractionManager
{
	//TODO: Something more pretty
	public Dictionary<Type, IRoomItemInteractionHandler> CreateHandlers(IPrivateRoom room);

	public bool TryGetHandler(IFurniture furniture, [NotNullWhen(true)] out Type? handler);
}
