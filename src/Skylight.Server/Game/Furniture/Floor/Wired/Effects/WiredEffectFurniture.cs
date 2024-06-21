using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Effects;

internal abstract class WiredEffectFurniture(int id, FloorFurnitureType type, Point2D dimensions, double height) : FloorFurniture(id, type, dimensions), IWiredEffectFurniture
{
	public override double DefaultHeight => height;
}
