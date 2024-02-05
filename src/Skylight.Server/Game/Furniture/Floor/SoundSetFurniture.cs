using System.Collections.Frozen;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundSetFurniture(int id, int width, int length, double height, int soundSetId, FrozenSet<int> samples) : FloorFurniture(id, width, length), ISoundSetFurniture
{
	public override double DefaultHeight => height;

	public int SoundSetId { get; } = soundSetId;

	public FrozenSet<int> Samples { get; } = samples;
}
