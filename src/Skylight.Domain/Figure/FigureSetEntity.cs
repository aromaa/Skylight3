using Skylight.Domain.Permissions;

namespace Skylight.Domain.Figure;

public class FigureSetEntity
{
	public int Id { get; init; }

	public int SetTypeId { get; set; }
	public FigureSetTypeEntity? SetType { get; set; }

	public FigureSexType? Sex { get; set; }

	public string? RankId { get; set; }
	public RankEntity? Rank { get; set; }

	public bool Selectable { get; set; }
	public bool Complimentary { get; set; }

	public List<FigureSetPartEntity>? Parts { get; set; }
	public List<FigureSetHiddenLayerEntity>? HiddenLayers { get; set; }
}
