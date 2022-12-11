using System.Collections.Immutable;

namespace Skylight.API.Game.Recycler.FurniMatic;

public interface IFurniMaticPrizeLevel
{
	public int Level { get; }
	public int Odds { get; }

	public ImmutableArray<IFurniMaticPrize> Prizes { get; }
}
