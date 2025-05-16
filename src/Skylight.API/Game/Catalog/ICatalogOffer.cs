using System.Collections.Immutable;
using Skylight.API.Game.Catalog.Products;

namespace Skylight.API.Game.Catalog;

public interface ICatalogOffer
{
	public int Id { get; }

	public string Name { get; }

	public int ClubRank { get; }

	public int CostCredits { get; }
	public int CostActivityPoints { get; }
	public int ActivityPointsType { get; }

	public TimeSpan RentTime { get; }

	public bool HasOffer { get; }

	public ImmutableArray<ICatalogProduct> Products { get; }

	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken = default);
}
