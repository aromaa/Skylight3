using Skylight.Domain.Furniture;

namespace Skylight.Domain.Recycler.FurniMatic;

public class FurniMaticWallItemEntity : FurniMaticItemEntity
{
	public int FurnitureId { get; set; }
	public WallFurnitureEntity? Furniture { get; set; }
}
