using Skylight.Domain.Permissions;

namespace Skylight.Domain.Catalog;

public class CatalogPageAccessEntity
{
	public int Id { get; init; }

	public int PageId { get; set; }
	public CatalogPageEntity? Page { get; set; }

	public string Partition { get; set; } = null!;
	public OperationType Operation { get; set; }

	public string RankId { get; set; } = null!;
	public RankEntity? Rank { get; set; }

	public enum OperationType
	{
		And,
		Or
	}
}
