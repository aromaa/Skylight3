using System.Collections.Immutable;

namespace Skylight.API.Game.Furniture.Floor;

public interface ISoundSetFurniture : IFloorFurniture
{
	public int SoundSetId { get; }

	public ImmutableHashSet<int> Samples { get; }
}
