namespace Skylight.Domain.Figure;

public class FigureSetHiddenLayerEntity
{
	public int SetId { get; set; }
	public FigureSetEntity? Set { get; set; }

	public int PartTypeId { get; set; }
	public FigurePartTypeEntity? PartType { get; set; }
}
