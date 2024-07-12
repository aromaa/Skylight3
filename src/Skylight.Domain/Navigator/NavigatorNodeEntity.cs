namespace Skylight.Domain.Navigator;

public abstract class NavigatorNodeEntity
{
	public int Id { get; init; }

	public int? ParentId { get; set; }
	public NavigatorNodeEntity? Parent { get; set; }

	public string Caption { get; init; } = null!;

	public List<NavigatorNodeEntity>? Children { get; set; }
}
