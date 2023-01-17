using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;

namespace Skylight.Server.Game.Rooms.Items.Interactions;

internal sealed class RoomItemInteractionManager : IRoomItemInteractionManager
{
	public RoomItemInteractionManager()
	{
	}

	public Dictionary<Type, IRoomItemInteractionHandler> CreateHandlers(IRoom room)
	{
		Dictionary<Type, IRoomItemInteractionHandler> handlers = new()
		{
			[typeof(IStickyNoteInteractionHandler)] = new StickyNoteInteractionHandler(),
			[typeof(ISoundMachineInteractionManager)] = new SoundMachineInteractionHandler(),
			[typeof(IRollerInteractionHandler)] = new RollerInteractionHandler(room)
		};

		return handlers;
	}

	public bool TryGetHandler(IFurniture furniture, [NotNullWhen(true)] out Type? handler)
	{
		//TODO: Query from handlers
		if (furniture is IStickyNotePoleFurniture or IStickyNoteFurniture)
		{
			handler = typeof(IStickyNoteInteractionHandler);

			return true;
		}
		else if (furniture is ISoundMachineFurniture)
		{
			handler = typeof(ISoundMachineInteractionManager);

			return true;
		}
		else if (furniture is IRollerFurniture)
		{
			handler = typeof(IRollerInteractionHandler);

			return true;
		}

		handler = null;

		return false;
	}
}
