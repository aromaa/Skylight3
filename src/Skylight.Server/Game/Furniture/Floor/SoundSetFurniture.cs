using System.Collections.Immutable;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundSetFurniture : FloorFurniture, ISoundSetFurniture
{
	public int SoundSetId { get; }

	public ImmutableHashSet<int> Samples { get; }

	public SoundSetFurniture(int id, int width, int length, double height, int soundSetId, ImmutableHashSet<int> samples)
		: base(id, width, length, height)
	{
		this.SoundSetId = soundSetId;

		this.Samples = samples;
	}
}
