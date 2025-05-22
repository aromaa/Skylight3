using System.Collections.Frozen;
using Skylight.API.Game.Furniture.Floor;
using Skylight.API.Numerics;

namespace Skylight.Server.Game.Furniture.Floor;

internal sealed class SoundSetFurniture(int id, FloorFurnitureKind kind, Point2D dimensions, double height, int soundSetId, FrozenSet<int> samples) : PlainFloorFurniture(id, kind, dimensions, height), ISoundSetFurniture
{
	public int SoundSetId { get; } = soundSetId;

	public FrozenSet<int> Samples { get; } = samples;
}
