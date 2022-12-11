namespace Skylight.Domain.Recycler.FurniMatic;

public class FurniMaticPrizeLevelEntity
{
	public int Level { get; init; }
	public int Odds { get; set; }

	public List<FurniMaticPrizeEntity>? Prizes { get; set; }
}
