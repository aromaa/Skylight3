using System.Collections.Frozen;

namespace Skylight.API.Game.Furniture.Floor;

public interface ISoundSetFurniture : IPlainFloorFurniture
{
	public int SoundSetId { get; }

	public FrozenSet<int> Samples { get; }
}
