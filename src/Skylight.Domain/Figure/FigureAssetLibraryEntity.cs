namespace Skylight.Domain.Figure;

public class FigureAssetLibraryEntity
{
	public int Id { get; init; }

	public string FileName { get; set; } = null!;

	public int Revision { get; set; }

	public List<FigureAssetPartEntity>? AssetParts { get; set; }
}
