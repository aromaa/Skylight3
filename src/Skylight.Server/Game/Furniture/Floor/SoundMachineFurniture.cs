using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundMachineFurniture(int id, int width, int length, double height) : FixedHeightStatefulFloorFurniture(id, width, length, height), ISoundMachineFurniture
{
	public int SoundSetSlotCount => 4;
}
