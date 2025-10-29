namespace Skylight.Domain.Permissions;

public class AccessSetEntity
{
	public int Id { get; init; }

	public string Name { get; set; } = null!;

	public List<AccessSetRuleEntity>? Rules { get; set; }
}
