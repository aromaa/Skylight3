using System.Collections.Immutable;
using Skylight.API.Game.Catalog.Products;
using Skylight.API.Game.Users;

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

	public bool CanPurchase(IUser user) => true;

	public ValueTask PurchaseAsync(ICatalogTransaction transaction, CancellationToken cancellationToken = default);
}
