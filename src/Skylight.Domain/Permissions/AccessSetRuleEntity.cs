namespace Skylight.Domain.Permissions;

public abstract class AccessSetRuleEntity
{
	public int Id { get; init; }

	public int SetId { get; set; }
	public AccessSetEntity? Set { get; set; }
}
