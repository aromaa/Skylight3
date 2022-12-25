namespace Skylight.Domain.Recycler.FurniMatic;

public abstract class FurniMaticItemEntity
{
	public int Id { get; init; }

	public int PrizeId { get; set; }
	public FurniMaticPrizeEntity? Prize { get; set; }
}
