using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundMachineFurniture : FloorFurniture, ISoundMachineFurniture
{
	public SoundMachineFurniture(int id, int width, int length, double height)
		: base(id, width, length, height)
	{
	}

	public int SoundSetSlotCount => 4;
}
