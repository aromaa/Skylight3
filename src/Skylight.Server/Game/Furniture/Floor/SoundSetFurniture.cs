using System.Collections.Frozen;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundSetFurniture : FloorFurniture, ISoundSetFurniture
{
	public int SoundSetId { get; }

	public FrozenSet<int> Samples { get; }

	public SoundSetFurniture(int id, int width, int length, double height, int soundSetId, FrozenSet<int> samples)
		: base(id, width, length, height)
	{
		this.SoundSetId = soundSetId;

		this.Samples = samples;
	}
}
