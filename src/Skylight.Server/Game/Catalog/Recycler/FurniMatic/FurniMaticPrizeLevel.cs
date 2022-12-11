using System.Collections.Immutable;
using Skylight.API.Game.Recycler.FurniMatic;

namespace Skylight.Server.Game.Catalog.Recycler.FurniMatic;

internal sealed class FurniMaticPrizeLevel : IFurniMaticPrizeLevel
{
	public int Level { get; }
	public int Odds { get; }

	public ImmutableArray<IFurniMaticPrize> Prizes { get; }

	internal FurniMaticPrizeLevel(int level, int odds, ImmutableArray<IFurniMaticPrize> prizes)
	{
		this.Level = level;
		this.Odds = odds;

		this.Prizes = prizes;
	}
}
