namespace Skylight.Domain.Recycler.FurniMatic;

public class FurniMaticPrizeEntity
{
	public int Id { get; init; }

	public int Level { get; set; }
	public FurniMaticPrizeLevelEntity? PrizeLevel { get; set; }

	public string Name { get; set; } = null!;

	public List<FurniMaticItemEntity>? Items { get; set; }
}
