namespace Skylight.Domain.Figure;

public class FigureAssetPartEntity
{
	public int Id { get; init; }

	public int AssetLibraryId { get; set; }
	public FigureAssetLibraryEntity? AssetLibrary { get; set; }

	public int PartId { get; set; }
	public FigurePartEntity? Part { get; set; }
}
