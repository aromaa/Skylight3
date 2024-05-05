using System.Diagnostics.CodeAnalysis;
using Skylight.API.Game.Furniture;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Game.Furniture.Floor.Wired.Triggers;
using Skylight.API.Game.Furniture.Wall;
using Skylight.API.Game.Rooms;
using Skylight.API.Game.Rooms.Items.Interactions;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.API.Game.Rooms.Items.Interactions.Wired.Triggers;
using Skylight.Server.Game.Rooms.Items.Interactions.Wired.Effects;
using Skylight.Server.Game.Rooms.Items.Interactions.Wired.Triggers;

namespace Skylight.Server.Game.Rooms.Items.Interactions;

internal sealed class RoomItemInteractionManager : IRoomItemInteractionManager
{
	public Dictionary<Type, IRoomItemInteractionHandler> CreateHandlers(IRoom room)
	{
		WiredEffectInteractionHandler wiredInteractionHandler = new(room);

		Dictionary<Type, IRoomItemInteractionHandler> handlers = new()
		{
			[typeof(IStickyNoteInteractionHandler)] = new StickyNoteInteractionHandler(),
			[typeof(ISoundMachineInteractionManager)] = new SoundMachineInteractionHandler(),
			[typeof(IRollerInteractionHandler)] = new RollerInteractionHandler(room),
			[typeof(IWiredEffectInteractionHandler)] = wiredInteractionHandler,
			[typeof(IUnitSayTriggerInteractionHandler)] = new UnitSayTriggerInteractionHandler(wiredInteractionHandler),
			[typeof(IUnitEnterRoomTriggerInteractionHandler)] = new UnitEnterRoomTriggerInteractionHandler(wiredInteractionHandler),
			[typeof(IUnitUseItemTriggerInteractionHandler)] = new UnitUseItemTriggerInteractionHandler(wiredInteractionHandler)
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
		else if (furniture is IUnitSayTriggerFurniture)
		{
			handler = typeof(IUnitSayTriggerInteractionHandler);

			return true;
		}
		else if (furniture is IUnitEnterRoomTriggerFurniture)
		{
			handler = typeof(IUnitEnterRoomTriggerInteractionHandler);

			return true;
		}
		else if (furniture is IUnitUseItemTriggerFurniture)
		{
			handler = typeof(IUnitUseItemTriggerInteractionHandler);

			return true;
		}
		else if (furniture is IWiredEffectFurniture)
		{
			handler = typeof(IWiredEffectInteractionHandler);

			return true;
		}

		handler = null;

		return false;
	}
}
