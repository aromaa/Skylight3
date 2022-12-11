using Skylight.Domain.Items;

namespace Skylight.Domain.Recycler.FurniMatic;

public class FurniMaticGiftEntity
{
	public int ItemId { get; init; }
	public FloorItemEntity? Item { get; set; }

	public int PrizeId { get; set; }
	public FurniMaticPrizeEntity? Prize { get; set; }
}
