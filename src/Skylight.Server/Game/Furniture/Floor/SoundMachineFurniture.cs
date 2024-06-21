using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundMachineFurniture(int id, FloorFurnitureType type, Point2D dimensions, double height) : FixedHeightStatefulFloorFurniture(id, type, dimensions, height), ISoundMachineFurniture
{
	public int SoundSetSlotCount => 4;
}
