using Skylight.API.Game.Furniture;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Items.Interactions;

public interface IRoomItemInteractionHandler
{
	public bool CanPlaceItem(IFurniture furniture, Point2D location);
}
