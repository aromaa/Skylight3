using System.Collections.Frozen;
using Skylight.API.Game.Furniture.Floor;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundSetFurniture(int id, int width, int length, double height, int soundSetId, FrozenSet<int> samples) : PlainFloorFurniture(id, width, length, height), ISoundSetFurniture
{
	public int SoundSetId { get; } = soundSetId;

	public FrozenSet<int> Samples { get; } = samples;
}
