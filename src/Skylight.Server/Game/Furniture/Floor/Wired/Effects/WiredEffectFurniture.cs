using Skylight.API.Game.Furniture.Floor.Wired.Effects;

namespace Skylight.Server.Game.Furniture.Floor.Wired.Effects;

internal abstract class WiredEffectFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), IWiredEffectFurniture
{
	public override double DefaultHeight => height;
}
