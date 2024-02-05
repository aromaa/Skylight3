using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundMachineFurniture(int id, int width, int length, double height) : FloorFurniture(id, width, length), ISoundMachineFurniture
{
	public override double DefaultHeight => height;

	public int SoundSetSlotCount => 4;
}
