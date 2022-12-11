using System.Collections.Immutable;

namespace Skylight.API.Game.Recycler.FurniMatic;

public interface IFurniMaticPrizes
{
	public ImmutableArray<IFurniMaticPrizeLevel> Levels { get; }
}
