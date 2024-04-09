using Skylight.API.Game.Furniture.Floor.Wired.Triggers;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Triggers;

internal abstract class WiredTriggerFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IWiredTriggerFurniture
{
	public override double DefaultHeight => height;
}
