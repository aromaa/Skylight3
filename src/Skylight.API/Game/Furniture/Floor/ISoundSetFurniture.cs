using System.Collections.Frozen;

namespace Skylight.API.Game.Furniture.Floor;

public interface ISoundSetFurniture : IFloorFurniture
{
	public int SoundSetId { get; }

	public FrozenSet<int> Samples { get; }
}
