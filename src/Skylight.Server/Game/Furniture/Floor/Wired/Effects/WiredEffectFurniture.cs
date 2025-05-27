using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Game.Furniture.Floor.Wired.Effects;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Effects;

internal abstract class WiredEffectFurniture(int id, IFloorFurnitureKind kind, Point2D dimensions, double height) : FloorFurniture(id, kind, dimensions), IWiredEffectFurniture
{
	public override double DefaultHeight => height;
}
