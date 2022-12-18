using Skylight.Domain.Furniture;

namespace Skylight.Domain.Recycler.FurniMatic;

public class FurniMaticFloorItemEntity : FurniMaticItemEntity
{
	public int FurnitureId { get; set; }
	public FloorFurnitureEntity? Furniture { get; set; }
}
