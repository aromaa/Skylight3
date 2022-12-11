using Skylight.Domain.Furniture;

namespace Skylight.Domain.Recycler.FurniMatic;

public class FurniMaticItemEntity
{
	public int Id { get; init; }

	public int PrizeId { get; set; }
	public FurniMaticPrizeEntity? Prize { get; set; }

	public int? FloorFurnitureId { get; set; }
	public FloorFurnitureEntity? FloorFurniture { get; set; }

	public int? WallFurnitureId { get; set; }
	public WallFurnitureEntity? WallFurniture { get; set; }
}
